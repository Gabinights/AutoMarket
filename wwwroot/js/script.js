// AutoMarket JS
// Interatividade inicial (abas, formulários, etc.)

document.addEventListener('DOMContentLoaded', function() {
    // Alternar abas Login/Registo
    const tabs = document.querySelectorAll('.tab-btn');
    const panes = document.querySelectorAll('.tab-pane');
    tabs.forEach(tab => {
        tab.addEventListener('click', function() {
            tabs.forEach(t => t.classList.remove('active'));
            this.classList.add('active');
            panes.forEach(pane => pane.classList.remove('active'));
            document.getElementById(this.dataset.target).classList.add('active');
        });
    });
});

// Placeholder para validação de formulários (Simples)
function validarFormLogin() {
    // TODO: Adicionar validações reais
    return true;
}

function validarFormRegisto() {
    // TODO: Adicionar validações reais
    return true;
}

// TODO: Implementar fetch() para buscar veículos da API (RF1)
// TODO: Enviar dados do formulário 'criar-anuncio' para o backend (RF12)


