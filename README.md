# RDStationSharp 2023

A API **RDStationSharp** foi escrita em C# para permitir a Autenticação e comunicação com a **API REST JSON da RD Station Marketing**.

Através dela, podemos realizar a integração do ERP ou CRM com a RD Station, enviando novas conversões, atualizando Leads e etc.

Desenvolvi a API baseado na documentação oficial da RD: https://developers.rdstation.com/reference/introducao-rdsm

## Documentação

Quase 100% das classes e métodos da API estão documentados via sumarização XML nos códigos, para facilitar o entendimento.

## Funcionamento (WPF)
Escrevi essa API para ser facilmente integrada em aplicações WPF, mas irá funcionar tranquilamente em outros modelos de aplicação.

Quando montei a API aqui para o CRM da empresa, a ideia era realizar uma atualização completa uma vez ao dia, após o expediente. Portanto, ele pega toda a nossa base do CRM e sincroniza com a RD Station. Para isso, montei a classe principal RDStationSharp. No seu cenário ela pode não ser útil, mas a mantive em partes no projeto, para você entender melhor o fluxo de trabalho da API.

Na classe **RD_CustomFields** você pode incluir os campos personalizados da respectiva conta RD. Eu construi a API de forma que seja fácil trabalhar com esses custom fields.

Todo código foi escrito de forma síncrona, mas pode ser migrado para sua versão assíncrona facilmente.

Para realização da autenticação do tipo **OAuth2** exigido para logar na RD Station, criei um projeto WPF App separado que embute um **CEFSharp Browser** (Wrapper do Chrome para WPF).

Ali ele irá abrir uma janela e solicitar o login e autorização do usuário. O mesmo vale para a questão do Facebook ADS.

## Bônus
Coloquei algumas classes bônus nela, como a classe **RDCloud** que nos permite baixar e realizar operações de limpeza de base em toda a base de contatos da RD Station.

Outra classe surreal é a classe **FBCustomAudienceManagerSync** que permite fazer uma sincronização entre a Base da RD Station e um Público Personalizado do Facebook ADS, para facilitar ações de Remarketing.

Essa API inclui meu pacote base para trabalhar com MVVM, o namespace HL.MVVM.
Ou seja, de cara você pode começar a trabalhar com MVVM utilizando a minha API.

Além disso tudo, essa API conta com um sistema de Log em formato CSV, para facilitar o acompanhamento das sincronizações.

A classe Status do namespace Status é uma classe Singleton na qual toda a API faz notificações de status de cada tarefa que está sendo executada.

## Status

Desenvolvi as principais interfaces com a API da RD. Esta API não é completa e ainda não dá suporte para todas as operações da RD Station, porém, está bem avançada e o "grosso" já foi desenvolvido. Tenho certeza que irá adiantar em muito o seu trabalho.

**Se você continuar o desenvolvimento da API, por favor compartilhe as adições aqui com a comunidade.**

Esta API é fruto de meses de trabalho e está disponível gratuitamente para você :)
