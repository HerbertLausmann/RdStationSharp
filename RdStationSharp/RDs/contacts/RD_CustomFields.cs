using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace RdStationSharp.RDs.contacts
{
    /// <summary>
    /// Essa é a classe base para todos os Tipos usados para comunicação de Leads entre RD e Local. No código fonte dela, você deve declarar todos os custom fields da conta RD Station com a qual se deseja comunicar
    /// </summary>
    public class RD_CustomFields
    {
        [JsonProperty("cf_site_data_cadastro_rapido")]
        public int? CfSiteDataCadastroRapido { get; set; }

        [JsonProperty("cf_site_data_modificacao_cadastro")]
        public int? CfSiteDataModificacaoCadastro { get; set; }

        [JsonProperty("cf_cnpj")]
        public string CfCnpj { get; set; }

        [JsonProperty("cf_codigo")]
        public int? CfCodigo { get; set; }

        [JsonProperty("cf_cd_cliente_agrupado")]
        public int? Cf_cd_cliente_agrupado { get; set; }

        [JsonProperty("cf_data_cadastro_crm")]
        public string CfDataCadastroCrm { get; set; }

        [JsonProperty("cf_data_cadastro_rapido_site")]
        public string CfDataCadastroRapidoSite { get; set; }

        [JsonProperty("cf_data_modificacao_site")]
        public string CfDataModificacaoSite { get; set; }

        [JsonProperty("cf_ddd")]
        public int? CfDdd { get; set; }

        [JsonProperty("cf_dias_sem_compra")]
        public int? CfDiasSemCompra { get; set; }

        [JsonProperty("cf_perfil_cliente")]
        public string CfPerfilCliente { get; set; }

        [JsonProperty("cf_qtd_compras")]
        public int? CfQtdCompras { get; set; }

        [JsonProperty("cf_ramo_de_atividade")]
        public string CfRamoDeAtividade { get; set; }

        [JsonProperty("cf_razao_social")]
        public string CfRazaoSocial { get; set; }

        [JsonProperty("cf_status_comercial")]
        public string CfStatusComercial { get; set; }

        [JsonProperty("cf_tipo_de_cadastro")]
        public string CfTipoDeCadastro { get; set; }

        [JsonProperty("cf_vendedor")]
        public string CfVendedor { get; set; }

        [JsonProperty("cf_gerente")]
        public string CfGerente { get; set; }

        [JsonProperty("cf_ultima_af")]
        public int? CfUltimaAf { get; set; }

        [JsonProperty("cf_a_rede_possui_compra")]
        public string Cf_a_rede_possui_compra { get; set; }

        [JsonProperty("cf_grandes_contas")]
        public string Cf_grandes_contas { get; set; }

        [JsonProperty("cf_inadimplente")]
        public string Cf_inadimplente { get; set; }

        [JsonProperty("cf_negociacao_aberta_ultimos_30_dias")]
        public string Cf_negociacao_aberta_ultimos_30_dias { get; set; }

        [JsonProperty("cf_potencial_de_portfolio")]
        public string cf_potencial_de_portfolio { get; set; }

        [JsonProperty("cf_linhas_comercializadas")]
        public string cf_linhas_comercializadas { get; set; }

    }
}
