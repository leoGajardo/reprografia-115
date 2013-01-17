﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Reprografia.Models.Account;
using Reprografia.lib;
using Microsoft.Office.Interop.Excel;
using System.IO;
using Reprografia.Models;
using Reprografia.Data.XL;

namespace Reprografia.BusinessLogic
{
    public static class AvaliacaoBL
    {
        private static Data.ReprografiaContext db = new Data.ReprografiaContext();
        public const double DIAS_CORRIDOS_PARA_AVALIAR = 10.0;

        /// <summary>
        /// Determina se <paramref name="user"/> possui alguma avaliação pendente
        /// </summary>
        /// <param name="user">usuário a ser analisado</param>
        /// <returns>true se alguma das Avaliações de <paramref name="user"/> não tiver sido avaliada</returns>
        public static bool PossuiAvaliacaoPendente(User user)
        {
            if (user == null)
                throw new ArgumentNullException("user", "user is null.");
            var solicitacoes = from s in db.Solicitacoes
                               where s.User.UserName == user.UserName
                               select s;

            //Verificar se avaliações estão pendentes
            return solicitacoes.Any(s => !s.Avaliacao.Avaliado);
        }

        public static Models.Avaliacao CriarAvaliacao()
        {
            return new Models.Avaliacao();
        }
        public static Models.Avaliacao CriarAvaliacao(Models.Solicitacao solicitacao)
        {
            var avaliacao = CriarAvaliacao();
            avaliacao.Avaliado = false;
            avaliacao.DataLimite = solicitacao.DataSolicitacao.AddDays(DIAS_CORRIDOS_PARA_AVALIAR);
            avaliacao.Solicitacao = solicitacao;

            foreach (var item in solicitacao.Itens)
            {
                avaliacao.ItensAvaliacao.Add(new Models.ItemAvaliacao(item)
                    {
                        Avaliacao = avaliacao
                    });
            }

            return avaliacao;
        }

        public static void EscreverXl(Models.Avaliacao avaliacao, string siteRoot, Stream destination)
        {
            var strategy = new AvaliacaoWriterStrategy(avaliacao);
            var writer = new XLWriter(Path.Combine(siteRoot, "Excel/Planilhas/"), strategy);
            writer.WriteAll(destination);
        }

        public static string ToXlString(this string value)
        {
            switch ((AvaliacaoNotaEnum)value[0])
            {
                case AvaliacaoNotaEnum.A:
                    return "A";
                case AvaliacaoNotaEnum.X:
                    return "X";
                case AvaliacaoNotaEnum.NA:
                    return "NA";
                default:
                    throw new ArgumentException("Valor fora da enumeração");
                //return "NA";
            }
        }
    }
}