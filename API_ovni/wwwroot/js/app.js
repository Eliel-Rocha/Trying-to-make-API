// CORRIGIDO: Usando HTTPS e porta 7199 para garantir compatibilidade com o Kestrel
const BASE_URL = "https://localhost:7199/api";
const READ_ENDPOINT = "/OvniData/DataPorPeriodo";
const WRITE_ENDPOINT = "/OvniData/InserirDocumento";
const KEY_REQUEST_ENDPOINT = "/KeyRequest/GenerateKey";
const CONFIG_ENDPOINT = "/Admin/ConfiguracaoLimpeza";
const EXECUTE_CLEANUP_ENDPOINT = "/Admin/ExecutarLimpeza";


//geração da chave de usuário (Key Request) - disponível para qualquer pessoa (sem autenticação) public

async function generateUserKey() {
    const firstName = document.getElementById('firstName').value;
    const lastName = document.getElementById('lastName').value;
    const email = document.getElementById('email').value;
    const resultElement = document.getElementById('resultKeyRequest');
    const keyInputElement = document.getElementById('generatedUserKey');

    const url = `${BASE_URL}${KEY_REQUEST_ENDPOINT}`;
    resultElement.textContent = `Solicitando chave para ${email}...`;

    try {
        const response = await fetch(url, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                firstName: firstName,
                lastName: lastName,
                email: email
            })
        });

        const data = await response.json();

        if (response.ok) {
            resultElement.textContent = `SUCESSO (${response.status} OK) - Chave gerada com sucesso!`;
            keyInputElement.value = data.apiKey;
        } else {
            resultElement.textContent = `ERRO ${response.status}: ${data.message || JSON.stringify(data)}`;
            keyInputElement.value = 'Falha ao gerar chave.';
        }
    } catch (error) {
        resultElement.textContent = `ERRO DE CONEXÃO: ${error.message}`;
        keyInputElement.value = '';
    }
}


// teste de leitura de dados (GET) - Admin e User podem acessar

async function testReadData() {
    const apiKey = document.getElementById('apiKeyRead').value;
    const resultElement = document.getElementById('resultRead');

    // Usando datas fixas para o teste
    const dataInicio = "2025-01-01";
    const dataFim = "2025-12-31";
    const url = `${BASE_URL}${READ_ENDPOINT}?dataInicio=${dataInicio}&dataFim=${dataFim}`;

    resultElement.textContent = `Buscando em: ${url} com chave: ${apiKey.substring(0, 8)}...`;

    try {
        const response = await fetch(url, {
            method: 'GET',
            headers: { 'X-Api-Key': apiKey }
        });

        const contentType = response.headers.get("content-type");
        const isJson = contentType && contentType.indexOf("application/json") !== -1;
        const data = isJson ? await response.json() : await response.text();

        if (response.ok) {
            resultElement.textContent = `SUCESSO (${response.status} OK) - Acesso concedido (Role Admin/User).\n` +
                `${data.length} registros encontrados.\n\n` +
                JSON.stringify(data.slice(0, 2), null, 2) +
                (data.length > 2 ? `\n... Mais ${data.length - 2} ocultos.` : '');
        } else if (response.status === 401) {
            resultElement.textContent = ` FALHA DE AUTENTICAÇÃO (${response.status} Unauthorized).\n` +
                `Chave Inválida. Mensagem: ${data.message || data}.`;
        }
        else {
            resultElement.textContent = `ERRO NO SERVIDOR (${response.status}). Mensagem: ${isJson ? JSON.stringify(data) : data}`;
        }
    } catch (error) {
        resultElement.textContent = `ERRO DE CONEXÃO/REDE: ${error.message}`;
    }
}


// teste de inserção de dados (POST) - Apenas Admin pode acessar
async function testInsertBatch() {
    const apiKey = document.getElementById('apiKeyWrite').value;
    const resultElement = document.getElementById('resultWrite');
    const url = `${BASE_URL}${WRITE_ENDPOINT}`;

    // Documentos fictícios
    const newDocuments = [
        { "hex_id": "a0003", "flight": "POSTESTE", "lat": -17.0, "lon": -42.0, "alt_baro": 12000, "datetime": new Date().toISOString() }
    ];

    resultElement.textContent = `Enviando 1 documento para ${url} com chave: ${apiKey.substring(0, 8)}...`;

    try {
        const response = await fetch(url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'X-Api-Key': apiKey // Chave de Admin necessária
            },
            body: JSON.stringify(newDocuments)
        });

        const data = await response.json();

        if (response.ok) {
            resultElement.textContent = ` SUCESSO (${response.status} OK) - Acesso concedido (Role Admin).\n` +
                `Mensagem: ${data.message}`;
        } else if (response.status === 403) {
            resultElement.textContent = ` FALHA DE AUTORIZAÇÃO (${response.status} Forbidden).\n` +
                `Esta chave não tem a Role "Admin". (Política AdminOnly)`;
        }
        else {
            resultElement.textContent = ` ERRO NO SERVIDOR (${response.status}). Mensagem: ${data.message || JSON.stringify(data)}`;
        }
    } catch (error) {
        resultElement.textContent = `ERRO DE CONEXÃO/REDE: ${error.message}`;
    }
}

// --- 3. Gerenciamento de Configuração (Apenas Admin) ---

/**
 * (ADMIN) Obtém a configuração atual de limpeza (GET).
 */
async function getLimpezaConfig() {
    const apiKey = document.getElementById('apiKeyConfig').value;
    const resultElement = document.getElementById('resultConfig');
    const url = `${BASE_URL}${CONFIG_ENDPOINT}`;

    resultElement.textContent = `Buscando configuração em: ${url}...`;

    try {
        const response = await fetch(url, {
            method: 'GET',
            headers: { 'X-Api-Key': apiKey }
        });

        const data = await response.json();

        if (response.ok) {
            resultElement.textContent = ` Configuração Atual (${response.status} OK):\n` +
                JSON.stringify(data, null, 2);
            // Preenche os campos de atualização para facilitar
            document.getElementById('limiteBytes').value = data.limiteMaximoBytes;
            document.getElementById('percentualAlvo').value = data.percentualAlvoOcupacao;
        } else {
            resultElement.textContent = `ERRO ${response.status}: ${data.message || JSON.stringify(data)}`;
        }
    } catch (error) {
        resultElement.textContent = `ERRO DE CONEXÃO/REDE: ${error.message}`;
    }
}

/**
 * (ADMIN) Atualiza a configuração de limpeza (POST).
 */
async function updateLimpezaConfig() {
    const apiKey = document.getElementById('apiKeyConfig').value;
    const limiteBytes = document.getElementById('limiteBytes').value;
    const percentualAlvo = document.getElementById('percentualAlvo').value;
    const resultElement = document.getElementById('resultConfig');
    const url = `${BASE_URL}${CONFIG_ENDPOINT}`;

    // O DTO esperado pelo C# (ConfigUpdateDto)
    const updateDto = {
        LimiteMaximoBytes: parseInt(limiteBytes),
        PercentualAlvoOcupacao: parseFloat(percentualAlvo)
    };

    resultElement.textContent = `Atualizando configuração para Limite: ${limiteBytes} e Meta: ${percentualAlvo}...`;

    try {
        const response = await fetch(url, {
            method: 'POST', // O Controller usa POST para a atualização
            headers: {
                'Content-Type': 'application/json',
                'X-Api-Key': apiKey
            },
            body: JSON.stringify(updateDto)
        });

        const data = await response.json();

        if (response.ok) {
            resultElement.textContent = ` SUCESSO (${response.status} OK):\n` + data.message;
        } else {
            // 403 Forbidden se não for Admin
            resultElement.textContent = ` ERRO ${response.status}: ${data.message || JSON.stringify(data)}`;
        }
    } catch (error) {
        resultElement.textContent = ` ERRO DE CONEXÃO/REDE: ${error.message}`;
    }
}

/**
 * (ADMIN) Força a execução imediata da rotina de limpeza (POST).
 */
async function executeLimpeza() {
    const apiKey = document.getElementById('apiKeyConfig').value;
    const resultElement = document.getElementById('resultConfig');
    const url = `${BASE_URL}${EXECUTE_CLEANUP_ENDPOINT}`;

    resultElement.textContent = `Iniciando processo de limpeza forçada...`;

    try {
        const response = await fetch(url, {
            method: 'POST',
            headers: { 'X-Api-Key': apiKey }
        });

        const data = await response.json();

        if (response.ok) {
            resultElement.textContent = ` SUCESSO (${response.status} OK):\n` + data.message;
        } else {
            resultElement.textContent = ` ERRO ${response.status}: ${data.message || JSON.stringify(data)}`;
        }
    } catch (error) {
        resultElement.textContent = ` ERRO DE CONEXÃO/REDE: ${error.message}`;
    }
}