
// Configuração Global
const API_BASE_URL = window.location.origin;
let map;
let markersLayer; //  guardar os aviões e linhas
let dadosParaDownload = [];


document.addEventListener('DOMContentLoaded', () => {
    verificarAutenticacao();
    //inicializarMapa(); // Futuramente, para mostrar rotas no mapa
    configurarDatasPadrao();
});

function ExibirDados(lista) {

    const container = document.getElementById('json-container');

    container.innerHTML = ""; // Limpa a tela

    lista.forEach(voo => {
        const linha = document.createElement('div');

        
        linha.innerText = JSON.stringify(voo);

        
        linha.style.marginBottom = "10px";      
        linha.style.whiteSpace = "nowrap";      
        linha.style.fontFamily = "monospace";   
        linha.style.fontSize = "13px";
        linha.style.color = "#9cdcfe";          

        container.appendChild(linha);
    });
}

// Verificar Autenticação e Configurar Interface
function verificarAutenticacao() {
    const apiKey = localStorage.getItem('userApiKey');
    const userType = localStorage.getItem('userType'); // Pega o tipo definido no login

    if (!apiKey) {
        alert("Você precisa fazer login primeiro!");
        window.location.href = 'index.html';
        return;
    }

    const displayElement = document.getElementById('userDisplay');
    if (displayElement) {
        // Mostra os primeiros e últimos caracteres da chave para segurança
        const maskedKey = apiKey.length > 10 ? apiKey.substring(0, 6) + "..." + apiKey.substring(apiKey.length - 4) : apiKey;
        displayElement.innerText = "Chave: " + maskedKey;
    }

    
    if (userType === 'admin') {
        const userArea = document.querySelector('.user-area');

        if (!document.getElementById('btnAdminArea')) {
            const btnAdmin = document.createElement('button');
            btnAdmin.id = 'btnAdminArea';
            btnAdmin.innerText = "Admin"; 
            
            btnAdmin.style.marginRight = "15px";
            btnAdmin.style.padding = "6px 12px";
            btnAdmin.style.borderRadius = "4px";
            btnAdmin.style.border = "1px solid #0a58ca";
            btnAdmin.style.backgroundColor = "#0d6efd"; 
            btnAdmin.style.color = "#ffffff"; 
            btnAdmin.style.fontWeight = "500";
            btnAdmin.style.cursor = "pointer";
            btnAdmin.style.transition = "background-color 0.2s";

         
            btnAdmin.onmouseover = function () { this.style.backgroundColor = "#0b5ed7"; };
            btnAdmin.onmouseout = function () { this.style.backgroundColor = "#0d6efd"; };

            btnAdmin.onclick = function () {
                window.location.href = 'admin.html';
            };

            const btnLogout = document.querySelector('.btn-logout');
            if (userArea && btnLogout) {
                userArea.insertBefore(btnAdmin, btnLogout);
            }
        }
    }
}


function inicializarMapa() {
    
    map = L.map('map').setView([-14.235, -51.925], 4);

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '&copy; OpenStreetMap contributors'
    }).addTo(map);

    markersLayer = L.layerGroup().addTo(map);
}

// configura datas padrão
function configurarDatasPadrao() {
    const hoje = new Date().toISOString().split('T')[0];
    document.getElementById('dataInicio').value = hoje;
    document.getElementById('dataFim').value = hoje;
}

/*/  Buscar Dados na API: 
-> coleta os dados e exibe na tela /*/
async function buscarDados() {
    // Coleta as datas e elementos necessários
    const dtInicio = document.getElementById('dataInicio').value;
    const dtFim = document.getElementById('dataFim').value;
    const statusMsg = document.getElementById('statusMsg');
    const apiKey = localStorage.getItem('userApiKey');
    const btnDownload = document.getElementById('btnDownload');

    // Validação básica das datas
    if (!dtInicio || !dtFim) {
        alert("Selecione as datas!");
        return;
    }

    statusMsg.innerText = "Buscando dados...";
    statusMsg.style.color = "#0d6efd";

    //markersLayer.clearLayers();
    btnDownload.disabled = true;


    // Chamada à API para buscar os dados
    try {
        const url = `${API_BASE_URL}/api/OvniData/DataPorPeriodo?dataInicio=${dtInicio}&dataFim=${dtFim}`;

        const response = await fetch(url, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json',
                'X-Api-Key': apiKey 
            }
        });

        // Verifica se a resposta foi bem-sucedida
        if (!response.ok) {
            
            if (response.status === 401 || response.status === 403) {
                throw new Error("Chave de API inválida ou expirada.");
            }
            throw new Error(`Erro na API: ${response.status}`);
        }

        // Processa os dados recebidos
        const listaVoos = await response.json();
        dadosParaDownload = listaVoos;

        // Atualiza o total de registros encontrados
        document.getElementById('totalVoos').innerText = listaVoos.length;
        statusMsg.innerText = `${listaVoos.length} registros encontrados.`;
        statusMsg.style.color = "green";

        ExibirDados(listaVoos);
        //plotarNoMapa(listaVoos); futaramente,para mostrar rotas no mpa
        

        if (listaVoos.length > 0) btnDownload.disabled = false;

    } catch (error) {
        console.error(error);
        statusMsg.innerText = error.message;
        statusMsg.style.color = "red";
        if (error.message.includes("Chave")) {
            alert("Sua sessão expirou ou a chave é inválida. Faça login novamente.");
            logout();
        }
    }
}

/*/para plotra possivel rota do objeto no mapa, apenas teste, futuramente para mostrar rotas no mapa
function plotarNoMapa(lista) {
    if (!lista || lista.length === 0) {
        alert("Nenhum voo encontrado neste período.");
        return;
    }

    let pathCoords = [];

    lista.forEach(voo => {
        const lat = voo.lat || voo.latitude;
        const lon = voo.lon || voo.longitude;
        const id = voo.hex_id || voo.hex || 'N/A';
        const alt = voo.alt_baro || voo.geoAltitude || 0;
        const dataHora = new Date(voo.datetime || voo.data || voo.timestamp).toLocaleString();

        if (lat && lon) {
            pathCoords.push([lat, lon]);

            
            L.circleMarker([lat, lon], {
                radius: 5,
                color: '#0d6efd',
                fillColor: '#0d6efd',
                fillOpacity: 0.8,
                weight: 1
            })
                .bindPopup(
                    `<b>ID:</b> ${id}<br>
                 <b>Data:</b> ${dataHora}<br>
                 <b>Alt:</b> ${alt} ft`
                )
                .addTo(markersLayer);
        }
    });

   
    if (pathCoords.length > 1) {
        const flightPath = L.polyline(pathCoords, {
            color: '#0d6efd',
            weight: 3,
            opacity: 0.7,
            dashArray: '10, 15',
            lineJoin: 'round'
        }).addTo(markersLayer);

        map.fitBounds(flightPath.getBounds(), { padding: [50, 50] });
    } else if (pathCoords.length === 1) {
        map.setView(pathCoords[0], 10);
    }
}*/

// Baixar Relatório em CSV
function baixarRelatorio() {
    if (!dadosParaDownload || dadosParaDownload.length === 0) {
        alert("Nada para baixar.");
        return;
    }

    let csvContent = "data:text/csv;charset=utf-8,Data/Hora,Hex ID,Latitude,Longitude,Altitude\n";

    dadosParaDownload.forEach(voo => {
        const data = new Date(voo.datetime || voo.data || voo.timestamp).toLocaleString();
        const lat = voo.lat || voo.latitude || 0;
        const lon = voo.lon || voo.longitude || 0;
        const alt = voo.alt_baro || voo.geoAltitude || 0;
        const id = voo.hex_id || voo.hex || 'N/A';

        const row = [
            `"${data}"`, `"${id}"`, lat, lon, alt
        ].join(",");
        csvContent += row + "\r\n";
    });

    const encodedUri = encodeURI(csvContent);
    const link = document.createElement("a");
    link.setAttribute("href", encodedUri);
    link.setAttribute("download", `relatorio_rotas_${new Date().getTime()}.csv`);
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
}

// Logout
function logout() {
    localStorage.clear();
    window.location.href = 'index.html';
}