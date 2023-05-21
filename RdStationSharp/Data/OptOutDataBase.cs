using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using HL.MVVM;
using RdStationSharp.RDs.contacts;

namespace RdStationSharp.Data
{
    /// <summary>
    /// Classe OptOutDataBase que gerencia informações de opt-out de leads e implementa a base de dados de texto simples.
    /// </summary>
    public class OptOutDataBase : HL.MVVM.Data.SimpleTextDB
    {
        private static OptOutDataBase _Default;

        /// <summary>
        /// Construtor para inicializar a base de dados de opt-out.
        /// </summary>
        public OptOutDataBase()
        {
            Init("optout");
        }

        /// <summary>
        /// Instância singleton da classe OptOutDataBase.
        /// </summary>
        public static OptOutDataBase Default
        {
            get
            {
                if (_Default is null)
                {
                    _Default = new OptOutDataBase();
                    _Default.EnsureLoad();
                }
                return _Default;
            }
        }

        /// <summary>
        /// Verifica se o endereço de e-mail está na lista de opt-out.
        /// </summary>
        /// <param name="mailAddress">Endereço de e-mail a ser verificado.</param>
        /// <returns>Retorna verdadeiro se estiver na lista de opt-out, caso contrário, retorna falso.</returns>
        public bool IsOptOut(string mailAddress)
        {
            lock (this)
            {
                if (string.IsNullOrWhiteSpace(mailAddress)) return true;
                return DataBase.Contains(mailAddress.Trim());
            }
        }

        /// <summary>
        /// Verifica se o endereço de e-mail está na lista de opt-in.
        /// </summary>
        /// <param name="mailAddress">Endereço de e-mail a ser verificado.</param>
        /// <returns>Retorna verdadeiro se estiver na lista de opt-in, caso contrário, retorna falso.</returns>
        public bool IsOptIn(string mailAddress)
        {
            lock (this)
            {
                if (string.IsNullOrWhiteSpace(mailAddress)) return false;
                return !DataBase.Contains(mailAddress.Trim());
            }
        }

        /// <summary>
        /// Adiciona o endereço de e-mail à lista de opt-out.
        /// </summary>
        /// <param name="mailAddress">Endereço de e-mail a ser adicionado.</param>
        /// <returns>Retorna verdadeiro se adicionado com sucesso, caso contrário, retorna falso.</returns>
        public bool OptOutLead(string mailAddress)
        {
            lock (this)
            {
                if (string.IsNullOrWhiteSpace(mailAddress)) return false;
                if (!DataBase.Contains(mailAddress.Trim()))
                {
                    DataBase.Add(mailAddress.Trim());
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Remove o endereço de e-mail da lista de opt-out.
        /// </summary>
        /// <param name="mailAddress">Endereço de e-mail a ser removido.</param>
        /// <returns>Retorna verdadeiro se removido com sucesso, caso contrário, retorna falso.</returns>
        public bool OptInLead(string mailAddress)
        {
            lock (this)
            {
                if (string.IsNullOrWhiteSpace(mailAddress)) return false;
                if (DataBase.Contains(mailAddress.Trim()))
                {
                    DataBase.Remove(mailAddress.Trim());
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Verifica se o Lead está na lista de opt-out.
        /// </summary>
        /// <param name="Lead">Lead a ser verificado.</param>
        /// <returns>Retorna verdadeiro se estiver na lista de opt-out, caso contrário, retorna falso.</returns>
        public bool IsOptOut(LeadConversionData Lead)
        {
            lock (this)
            {
                if (Lead is null) return true;
                return IsOptOut(Lead.Payload.Email);
            }
        }

        /// <summary>
        /// Verifica se o Lead está na lista de opt-in.
        /// </summary>
        /// <param name="Lead">Lead a ser verificado.</param>
        /// <returns>Retorna verdadeiro se estiver na lista de opt-in, caso contrário, retorna falso.</returns>
        public bool IsOptIn(LeadConversionData Lead)
        {
            lock (this)
            {
                if (Lead is null) return false;
                return IsOptIn(Lead.Payload.Email);
            }

        }

        /// <summary>
        /// Adiciona o Lead à lista de opt-out.
        /// </summary>
        /// <param name="Lead">Lead a ser adicionado.</param>
        /// <returns>Retorna verdadeiro se adicionado com sucesso, caso contrário, retorna falso.</returns>
        public bool OptOutLead(LeadConversionData Lead)
        {
            lock (this)
            {
                if (Lead is null) return false;
                return OptOutLead(Lead.Payload.Email);
            }
        }

        /// <summary>
        /// Remove o Lead da lista de opt-out.
        /// </summary>
        /// <param name="Lead">Lead a ser removido.</param>
        /// <returns>Retorna verdadeiro se removido com sucesso, caso contrário, retorna falso.</returns>
        public bool OptInLead(LeadConversionData Lead)
        {
            lock (this)
            {
                if (Lead is null) return false;
                return OptInLead(Lead.Payload.Email);
            }
        }
    }
}

