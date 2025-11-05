O projeto tem como principal objetivo o desenvolvimento de uma aplicação web que suporte um portal de compra e venda de veículos usados, semelhante a plataformas existentes no mercado, como o StandVirtual ou o Auto.pt.



Levantamento dos requisitos funcionais:

\- RF1: O sistema deve permitir que visitantes consultem anúncios de veículos, com filtros por marca, modelo, ano, preço, quilometragem, combustível, transmissão e localização.



\- RF2: O sistema deve exibir a lista de veículos com detalhes (especificações, preço, descrição, imagens, estado do anúncio).



\- RF3: O sistema deve permitir visualizar o detalhe de cada anúncio, com galeria de imagens e dados completos do veículo.

RF4: O sistema deve permitir o registo e autenticação de compradores.



\- RF5: O comprador pode guardar filtros de pesquisa e definir marcas favoritas



\- RF7: O comprador pode agendar visitas a veículos, escolhendo data e hora.





\- RF9: O comprador pode consultar o histórico de reservas, visitas e encomendas.



\- RF10: O comprador pode apresentar denúncias sobre anúncios ou utilizadores.

\- RF11: O sistema deve permitir o registo e autenticação de vendedores (com aprovação por administrador).



\- RF12: O vendedor pode criar, editar, pausar, ativar e remover anúncios.



\- RF13: O vendedor pode carregar imagens para os seus anúncios.



\- RF14: O vendedor pode atualizar o estado de um anúncio (ativo, vendido, pausado).



\- RF15: O vendedor pode responder a mensagens ou denúncias relacionadas aos seus anúncios.



\- RF16: O vendedor pode consultar o histórico de  vendas dos seus anúncios.

\- RF17: O administrador pode aprovar ou bloquear vendedores, com registo do motivo.



\- RF18: O administrador pode ativar/bloquear utilizadores e consultar histórico de bloqueios.



\- RF19: O administrador pode moderar anúncios, alterando o seu estado ou removendo-os.



\- RF20: O administrador deve gerir denúncias, percorrendo o workflow Aberta → Em análise → Encerrada (procedente/não procedente).



\- RF21: O administrador pode registar ações em denúncias (atribuir, pedir info, encerrar, etc.).



\- RF22: O administrador pode enviar notificações a compradores e vendedores sobre alterações ou decisões.



\- RF23: O administrador pode consultar estatísticas e relatórios (utilizadores, anúncios ativos, vendas, reservas, denúncias, etc.).

\- RF24: O sistema deve registar em auditoria todas as ações de moderação e administração.



Levantamento dos requisitos não-funcionais:



\- RNF1: O sistema deve implementar autenticação segura e autorização baseada em papéis (comprador, vendedor, administrador).

\- RNF2: As passwords devem ser encriptadas e os dados pessoais tratados conforme o RGPD.





\- RNF3: Todas as ações administrativas devem ser registadas em logs de auditoria.



\- RNF4: A aplicação deve ser responsiva, adaptando-se a desktop, tablet e dispositivos móveis.



\- RNF5: O sistema deve fornecer feedback claro ao utilizador (mensagens de erro, estados de reserva/compra, confirmações). 

\- RNF6: O sistema deve suportar pelo menos 5.000 acessos concorrentes sem degradação significativa.



\- RNF7: Consultas e listagens de anúncios devem ter tempo de resposta inferior a 2 segundos em condições normais.



\- RNF8: O sistema deve garantir integridade dos dados através de constraints, transações e triggers.



\- RNF9: O sistema deve realizar backups diários e permitir recuperação em até 1 hora após falha.



\- RNF10: O sistema deve estar disponível em pelo menos 99% do tempo definido no SLA.



\- RNF11: O sistema deve ser modular e documentado, facilitando manutenção e evolução.



\- RNF12: O código deve seguir normas de estilo e ser acompanhado de testes unitários e de integração.



\- RNF13: O sistema deve permitir fácil portabilidade entre ambientes (dev/test/prod).



