using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HL.MVVM;
using RdStationSharp.RDs.contacts;

namespace RdStationSharp.Data
{
    /// <summary>
    /// Classe DisengagedDataBase que gerencia informações de contatos desengajados e implementa a base de dados de texto simples.
    /// </summary>
    public class DisengagedDataBase : HL.MVVM.Data.SimpleTextDB
    {

        private static DisengagedDataBase _SendinBlue;
        private static DisengagedDataBase _RDStation;

        /// <summary>
        /// Construtor privado para inicializar a base de dados de contatos desengajados.
        /// </summary>
        /// <param name="name">Nome da base de dados.</param>
        private DisengagedDataBase(string name = "disengaged")
        {
            Init(name);
        }

        /// <summary>
        /// Instância singleton da classe DisengagedDataBase para SendinBlue.
        /// </summary>
        public static DisengagedDataBase SendinBlue
        {
            get
            {
                if (_SendinBlue is null)
                {
                    _SendinBlue = new DisengagedDataBase();
                    _SendinBlue.EnsureLoad();
                }
                return _SendinBlue;
            }
        }

        /// <summary>
        /// Instância singleton da classe DisengagedDataBase para RDStation.
        /// </summary>
        public static DisengagedDataBase RDStation
        {
            get
            {
                if (_RDStation is null)
                {
                    _RDStation = new DisengagedDataBase("rdstation-disengaged");
                    _RDStation.EnsureLoad();
                }
                return _RDStation;
            }
        }

        /// <summary>
        /// Verifica se o endereço de e-mail está na lista de contatos desengajados.
        /// </summary>
        /// <param name="mailAddress">Endereço de e-mail a ser verificado.</param>
        /// <returns>Retorna verdadeiro se estiver na lista de desengajados, caso contrário, retorna falso.</returns>
        public bool IsDisengaged(string mailAddress)
        {
            lock (this)
            {
                if (string.IsNullOrWhiteSpace(mailAddress)) return true;
                return DataBase.Contains(mailAddress.Trim());
            }
        }

        /// <summary>
        /// Verifica se o endereço de e-mail está na lista de contatos engajados.
        /// </summary>
        /// <param name="mailAddress">Endereço de e-mail a ser verificado.</param>
        /// <returns>Retorna verdadeiro se estiver na lista de engajados, caso contrário, retorna falso.</returns>
        public bool IsEngaged(string mailAddress)
        {
            lock (this)
            {
                if (string.IsNullOrWhiteSpace(mailAddress)) return false;
                return !DataBase.Contains(mailAddress.Trim());
            }
        }

        /// <summary>
        /// Adiciona o endereço de e-mail à lista de contatos desengajados.
        /// </summary>
        /// <param name="mailAddress">Endereço de e-mail a ser adicionado.</param>
        /// <returns>Retorna verdadeiro se adicionado com sucesso, caso contrário, retorna falso.</returns>
        public bool Disengage(string mailAddress)
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
        /// Remove o endereço de e-mail da lista de contatos desengajados.
        /// </summary>
        /// <param name="mailAddress">Endereço de e-mail a ser removido.</param>
        /// <returns>Retorna verdadeiro se removido com sucesso, caso contrário, retorna falso.</returns>
        public bool Engage(string mailAddress)
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
    }
}

