const API_BASE_URL = window.location.origin;

// Variável para guardar os dados do cadastro temporariamente
let usuarioRecemCriado = null;


// 1. GERAR NOVA CHAVE (Cadastro)
async function gerarChave() {
    const nome = document.getElementById('inputNome').value;
    const sobrenome = document.getElementById('inputSobrenome').value;
    const email = document.getElementById('inputEmail').value;
    const feedback = document.getElementById('mensagemFeedback');

    //parte de validação simples para evitar requisições desnecessárias
    const boxSucesso = document.getElementById('resultadoChave');
    const displayChave = document.getElementById('displayChaveGerada');

    // Validação básica
    if (!email || !nome) {
        // Se faltar nome ou email, mostra mensagem de erro e para a função
        if (feedback) {
            feedback.innerText = "Por favor, preencha nome e email.";
            feedback.style.color = "red";
        }
        return;
    }

    // Feedback de carregamento
    if (feedback) {
        feedback.innerText = "Gerando chave...";
        feedback.style.color = "#004aad";
    }

    // Esconde a caixa de sucesso anterior se houver
    if (boxSucesso) boxSucesso.classList.add('d-none');

    try {
        const payload = {
            firstName: nome,
            lastName: sobrenome,
            email: email
        };

        const response = await fetch(`${API_BASE_URL}/api/KeyRequest/GenerateKey`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        });

        const data = await response.json();

        if (response.ok) {
            if (feedback) {
                feedback.innerText = ""; // Limpa msg de carregando
            }

            // Preenche o input de login automaticamente
            document.getElementById('inputApiKey').value = data.apiKey;

            // MOSTRA A ÁREA DE DOWNLOAD
            if (boxSucesso && displayChave) {
                boxSucesso.classList.remove('d-none');
                displayChave.innerText = data.apiKey;
            }

            // Salva na memória para o download funcionar
            usuarioRecemCriado = {
                nome: `${data.name}`, 
                email: data.email,
                chave: data.apiKey
            };

        } else {
            if (feedback) {
                feedback.innerText = "Erro: " + (data.message || "Falha ao gerar");
                feedback.style.color = "red";
            }
        }
    } catch (error) {
        console.error(error);
        if (feedback) {
            feedback.innerText = "Erro de conexão com o servidor.";
            feedback.style.color = "red";
        }
    }
}

// FUNÇÕES DE APOIO (DOWNLOAD E COPIAR)

function baixarChaveTxtLogin() {
    if (!usuarioRecemCriado) return;

    const conteudo = `

   CREDENCIAL DE ACESSO - SISTEMA OVNI

Data: ${new Date().toLocaleString()}

USUÁRIO:
Nome:  ${usuarioRecemCriado.nome}
Email: ${usuarioRecemCriado.email}

----------------------------------------
SUA CHAVE DE ACESSO:
${usuarioRecemCriado.chave}
----------------------------------------

COMO USAR:
1. Copie a chave acima.
2. Cole no campo "Insira sua Chave de Acesso".
3. Clique em Entrar.

Guarde este arquivo em local seguro.
`;

    const blob = new Blob([conteudo], { type: "text/plain" });
    const link = document.createElement("a");
    link.href = URL.createObjectURL(blob);

    // Nome do arquivo
    const nomeSafe = usuarioRecemCriado.nome.replace(/[^a-z0-9]/gi, '_').toLowerCase();
    link.download = `acesso_ovni_${nomeSafe}.txt`;

    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
}

// Função para copiar a chave para a área de transferência
function copiarChaveLogin() {
    const chave = document.getElementById('displayChaveGerada').innerText;
    if (!chave || chave === "...") return;

    navigator.clipboard.writeText(chave).then(() => {
        const btn = event.target;
        const textoOriginal = btn.innerText;

        btn.innerText = "Copiado!";
        btn.style.backgroundColor = "#d1e7dd";

        setTimeout(() => {
            btn.innerText = textoOriginal;
            btn.style.backgroundColor = ""; // Volta ao original
        }, 2000);
    });
}



// 2. ENTRAR NO SISTEMA

async function acessarSistema() {
    const chaveInput = document.getElementById('inputApiKey');
    const chave = chaveInput.value.trim();
    const feedback = document.getElementById('mensagemFeedback');
    const btnEntrar = document.querySelector('.btn-action');

    if (!chave) {
        alert("Por favor, insira uma chave válida.");
        return;
    }

    const textoOriginal = btnEntrar.innerText;
    btnEntrar.innerText = "Verificando..."; // Feedback visual 
    btnEntrar.disabled = true;
    if (feedback) feedback.innerText = "";

    // --- CONFIGURAÇÃO DO TIMEOUT ---
    const controller = new AbortController();
    const timeoutId = setTimeout(() => controller.abort(), 8000); 

    try {
        const url = `${API_BASE_URL}/api/KeyRequest/Validate?apiKey=${encodeURIComponent(chave)}`;

        const response = await fetch(url, {
            method: 'GET',
            headers: { 'Content-Type': 'application/json' },
            signal: controller.signal // Conecta o cronômetro à requisição
        });

        clearTimeout(timeoutId);

        if (response.ok) {
            const data = await response.json();
            localStorage.setItem('userApiKey', chave);
            localStorage.setItem('userType', data.role);

            if (data.role === 'admin') window.location.href = 'admin.html';
            else window.location.href = 'painel.html';

        } else if (response.status === 503) {
     
            alert("O banco de dados está acordando ou indisponível. Tente novamente em instantes.");

        } else if (response.status === 403) {
            const data = await response.json();
            if (data.canReactivate) {
                const aceitou = confirm(`Sua chave expirou em ${new Date(data.validade).toLocaleDateString()}. \nDeseja reativar agora?`);
                if (aceitou) await reativarChave(chave);
            } else {
                alert("Acesso negado: " + data.message);
            }
        } else {
            const erro = await response.json();
            alert("Acesso negado: " + (erro.message || "Chave inválida."));
        }

    } catch (error) {
        if (error.name === 'AbortError') {
            //  se passar dos 8 segundos configurados 
            alert("A conexão com o servidor demorou demais. Verifique se o banco no Atlas está ativo ou se sua VPN está ligada.");[cite: 61, 65]
        } else {
            console.error("Erro no login:", error);
            alert("Não foi possível conectar ao servidor. Verifique sua internet.");
        }
    } finally {
        btnEntrar.innerText = textoOriginal;
        btnEntrar.disabled = false;
    }
}

// RENOVAR A CHAVE
async function reativarChave(chave) {
    try {
        const response = await fetch(`${API_BASE_URL}/api/KeyRequest/Reactivate`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            // Como é string simples no Body, enviamos assim:
            body: JSON.stringify(chave)
        });

        if (response.ok) {
            alert(" Sucesso! Sua chave foi renovada. Tente entrar novamente.");
            acessarSistema(); 
        } else {
            alert("Erro ao tentar reativar. Contate o admin.");
        }
    } catch (e) {
        alert("Erro de conexão ao reativar.");
    }
}

document.getElementById('inputApiKey')?.addEventListener('keypress', function (e) {
    if (e.key === 'Enter') acessarSistema();
});