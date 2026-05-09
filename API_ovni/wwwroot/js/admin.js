const API_BASE_URL = window.location.origin;

// VARIÁVEL GLOBAL PARA O DOWNLOAD 
// Guarda os dados do último usuário criado para o botão de download funcionar
let ultimoUsuarioGerado = null;

// 1Ao carregar: Verifica nível de acesso e ajusta a tela
document.addEventListener('DOMContentLoaded', () => {
    configurarInterfacePorNivel();
});

// Controla quem vê o quê (Admin vs User)
function configurarInterfacePorNivel() {
    const apiKey = localStorage.getItem('userApiKey');
    const userType = localStorage.getItem('userType');

    // Se não tiver chave nenhuma, volta para a pagina inicial
    if (!apiKey) {
        alert("Acesso negado. Faça login primeiro.");
        window.location.href = 'index.html';
        return;
    }

    // Se for USUÁRIO COMUM 
    if (userType !== 'admin') {
     
        const cardUsers = document.getElementById('cardGestaoUsers');
        const cardBanco = document.getElementById('cardGestaoBanco');

        if (cardUsers) cardUsers.style.display = 'none';
        if (cardBanco) cardBanco.style.display = 'none';

        const titulo = document.querySelector('.logo-area span');
        if (titulo) titulo.innerText = "Área de Envio de Dados";

    } else {
       
        carregarConfiguracaoLimpeza();
    }
}

// =======================================================
// FUNCIONALIDADE 1: CRIAR USUÁRIO (Com Download e Cópia)
// =======================================================
async function criarUsuario() {
    const nome = document.getElementById('novoNome').value.trim();
    const email = document.getElementById('novoEmail').value.trim();
    const isAdmin = document.getElementById('checkIsAdmin').checked;

    // Elementos visuais
    const boxResultado = document.getElementById('resultadoChave');
    const displayChave = document.getElementById('displayChaveGerada');
    const apiKey = localStorage.getItem('userApiKey');

    if (!nome || !email) {
        alert("Preencha Nome e E-mail.");
        return;
    }

    const ownerInfo = `${nome},${email}`;

    try {
        const url = `${API_BASE_URL}/api/Admin/GerarChaveUsuario?grantAdmin=${isAdmin}`;

        const response = await fetch(url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'X-Api-Key': apiKey
            },
            body: JSON.stringify(ownerInfo)
        });

        if (response.ok) {
            const data = await response.json();

            
            boxResultado.classList.remove('d-none');
            displayChave.innerText = data.apiKey;

           
            ultimoUsuarioGerado = {
                nome: data.name,
                email: data.email,
                chave: data.apiKey,
                tipo: isAdmin ? "Administrador" : "Pesquisador/Estação"
            };

            alert(`Usuário ${data.name} criado com sucesso!`);

            // Limpa os campos de input
            document.getElementById('novoNome').value = '';
            document.getElementById('novoEmail').value = '';

        } else {
            const erro = await response.text();
            alert("Erro ao criar: " + erro);
        }

    } catch (error) {
        console.error(error);
        alert("Erro de conexão.");
    }
}

//  FUNÇÃO DE DOWNLOAD DO LOGIN (.txt)
function baixarChaveTxt() {
    if (!ultimoUsuarioGerado) {
        alert("Nenhuma chave foi gerada ainda.");
        return;
    }

    // Monta o texto do arquivo
    const conteudo = `
========================================
   CREDENCIAL DE ACESSO - SISTEMA OVNI
========================================
Gerado em: ${new Date().toLocaleString()}

USUÁRIO:
Nome:  ${ultimoUsuarioGerado.nome}
Email: ${ultimoUsuarioGerado.email}
Nível: ${ultimoUsuarioGerado.tipo}

----------------------------------------
SUA CHAVE DE LOGIN (API KEY):
${ultimoUsuarioGerado.chave}
----------------------------------------

COMO USAR:
1. Acesse o painel do sistema.
2. No campo "Chave de Acesso", cole o código acima.
3. Se for configurar um Raspberry Pi, coloque esta chave no script.

 MANTENHA ESTE ARQUIVO SEGURO.
`;

    const blob = new Blob([conteudo], { type: "text/plain" });
    const link = document.createElement("a");

    link.href = URL.createObjectURL(blob);

    // Nome do arquivo seguro
    const nomeSafe = ultimoUsuarioGerado.nome.replace(/[^a-z0-9]/gi, '_').toLowerCase();
    link.download = `login_ovni_${nomeSafe}.txt`;

    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
}

// FUNÇÃO DE COPIAR 
function copiarChave() {
    const chave = document.getElementById('displayChaveGerada').innerText;
    if (!chave || chave === "...") return;

    navigator.clipboard.writeText(chave).then(() => {
        const btn = event.target;
        const textoOriginal = btn.innerText;

        btn.innerText = "Copiado!";
        btn.classList.replace("btn-outline-primary", "btn-primary"); // Muda cor para azul cheio

        setTimeout(() => {
            btn.innerText = textoOriginal;
            btn.classList.replace("btn-primary", "btn-outline-primary"); // Volta ao normal
        }, 2000);
    });
}

// =======================================================
// FUNCIONALIDADE 2: CONFIGURAÇÃO DE LIMPEZA
// =======================================================
async function carregarConfiguracaoLimpeza() {
    const apiKey = localStorage.getItem('userApiKey');
    const displayConfig = document.getElementById('viewConfigDb');

    if (displayConfig) displayConfig.innerText = "Buscando...";

    try {
        const response = await fetch(`${API_BASE_URL}/api/Admin/ConfiguracaoLimpeza`, {
            method: 'GET',
            headers: { 'X-Api-Key': apiKey }
        });

        if (response.ok) {
            const dados = await response.json();
            const config = dados.config;
            const status = dados.status;

            // Visualização (Topo do Cartão)
            const mbTotal = (config.limiteMaximoBytes / (1024 * 1024)).toFixed(0);
            const metaPercent = (config.percentualAlvoOcupacao * 100).toFixed(0);

            if (displayConfig) displayConfig.innerText = `${mbTotal} MB | Meta: ${metaPercent}%`;

            // Inputs de Edição
            const inputBytes = document.getElementById('configBytes');
            const inputPercent = document.getElementById('configPercent');

            if (inputBytes) inputBytes.value = mbTotal;
            if (inputPercent) inputPercent.value = config.percentualAlvoOcupacao;

            // Barra de Progresso
            const barra = document.getElementById('progressoBanco');
            if (barra) {
                const pct = status.usoPorcentagem;
                barra.style.width = `${pct}%`;
                barra.innerText = `${pct}%`;

                barra.className = "progress-bar progress-bar-striped";
                if (pct < 50) barra.classList.add("bg-success");
                else if (pct < 80) barra.classList.add("bg-warning");
                else barra.classList.add("bg-danger");
            }

            // Textos auxiliares
            const txtBytes = document.getElementById('textoBytesAtuais');
            const txtLimit = document.getElementById('textoLimite');

            if (txtBytes) txtBytes.innerText = `Usado: ${(status.bytesAtuais / (1024 * 1024)).toFixed(2)} MB`;
            if (txtLimit) txtLimit.innerText = `Max: ${mbTotal} MB`;

        } else {
            if (displayConfig) displayConfig.innerText = "Erro ao carregar";
        }
    } catch (error) {
        console.error(error);
        if (displayConfig) displayConfig.innerText = "Sem conexão";
    }
}

async function salvarConfiguracao() {
    const mb = document.getElementById('configBytes').value;
    const percent = document.getElementById('configPercent').value;
    const apiKey = localStorage.getItem('userApiKey');

    if (!mb || !percent) {
        alert("Preencha todos os campos de configuração.");
        return;
    }

    const bytes = mb * 1024 * 1024;
    const payload = {
        limiteMaximoBytes: parseInt(bytes),
        percentualAlvoOcupacao: parseFloat(percent)
    };

    try {
        const response = await fetch(`${API_BASE_URL}/api/Admin/ConfiguracaoLimpeza`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'X-Api-Key': apiKey
            },
            body: JSON.stringify(payload)
        });

        if (response.ok) {
            alert("Configuração salva e aplicada!");
            carregarConfiguracaoLimpeza(); // Recarrega para ver a barra atualizar
        } else {
            alert("Erro ao salvar.");
        }
    } catch (error) {
        alert("Erro de conexão.");
    }
}

async function executarLimpeza() {
    if (!confirm("Isso apagará registros antigos para liberar espaço. Continuar?")) return;

    const apiKey = localStorage.getItem('userApiKey');
    try {
        const response = await fetch(`${API_BASE_URL}/api/Admin/ExecutarLimpeza`, {
            method: 'POST',
            headers: { 'X-Api-Key': apiKey }
        });

        if (response.ok) {
            alert("Limpeza solicitada. Verifique o console do servidor.");
            setTimeout(carregarConfiguracaoLimpeza, 2000); // Atualiza a barra após 2s
        } else {
            alert("Erro ao limpar.");
        }
    } catch (error) {
        alert("Erro de conexão.");
    }
}

// =======================================================
// FUNCIONALIDADE 3: ENVIO DE JSON (POST)
// =======================================================
async function enviarJsonBruto() {
    const jsonStr = document.getElementById('jsonRawInput').value.trim();
    const logDiv = document.getElementById('manualLog');
    const apiKey = localStorage.getItem('userApiKey');

    if (!jsonStr) {
        alert("A caixa de texto está vazia.");
        return;
    }

    logDiv.innerText = "Validando...";
    logDiv.style.color = "blue";

    let payload;
    try {
        let parsed = JSON.parse(jsonStr);
        if (!Array.isArray(parsed)) parsed = [parsed];
        payload = parsed;
    } catch (e) {
        logDiv.innerText = "Erro no JSON. Verifique aspas e vírgulas.";
        logDiv.style.color = "red";
        return;
    }

    logDiv.innerText = `Enviando ${payload.length} registros...`;

    try {
        const url = `${API_BASE_URL}/api/OvniData/InserirDocumento`;
        const response = await fetch(url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'X-Api-Key': apiKey
            },
            body: JSON.stringify(payload)
        });

        if (response.ok) {
            const data = await response.json();
            logDiv.innerText = `Sucesso! ${data.message}`;
            logDiv.style.color = "green";

            // Se for Admin, atualiza a barra de espaço pq acabou de encher o banco
            const userType = localStorage.getItem('userType');
            if (userType === 'admin') carregarConfiguracaoLimpeza();

        } else {
            const erro = await response.text();
            let msg = erro;
            try { msg = JSON.parse(erro).message || erro; } catch { }

            logDiv.innerText = "Erro: " + msg;
            logDiv.style.color = "red";
        }
    } catch (error) {
        logDiv.innerText = "Erro de conexão.";
        logDiv.style.color = "red";
    }
}

function logout() {
    localStorage.clear();
    window.location.href = 'index.html';
}