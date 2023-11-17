using myRaiCommonModel;
using myRaiData.Incentivi;
using myRaiHelper;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using CommonHelper = myRai.Business.CommonManager;

namespace myRai.Business
{
    public class GruppiAdManager
    {
        public static List<GruppoAd> GetGruppiAd()
        {
            List<GruppoAd> elencoGruppi = new List<GruppoAd>();

            var account = CommonHelper.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio_AD);

            using (PrincipalContext mainContext = new PrincipalContext(ContextType.Domain, "RAI", account[0], account[1]))
            {
                GruppoAd gruppoAd = null;

                //if (InternalGetGruppoAd(mainContext, "UT_RUO_INVIOMAIL", out gruppoAd))
                elencoGruppi.Add(new GruppoAd() { Nome = "UT_RUO_INVIOMAIL", Persone = new List<UserAD>() });

                //if (InternalGetGruppoAd(mainContext, "UT_SISTEMA_TRASFERTE", out gruppoAd))
                elencoGruppi.Add(new GruppoAd() { Nome = "UT_SISTEMA_TRASFERTE", Persone = new List<UserAD>() });
            }

            return elencoGruppi;
        }

        public static GruppoAd GetGruppoAd(string name)
        {
            GruppoAd gruppo = new GruppoAd();

            var account = CommonHelper.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio_AD);

            using (PrincipalContext mainContext = new PrincipalContext(ContextType.Domain, "RAI", account[0], account[1]))
            {
                InternalGetGruppoAd(mainContext, name, out gruppo);
            }

            return gruppo;
        }

        private static bool InternalGetGruppoAd(PrincipalContext mainContext, string groupName, out GruppoAd gruppoAd)
        {
            bool result = true;
            gruppoAd = new GruppoAd()
            {
                Nome = groupName,
                Persone = new List<UserAD>()
            };

            using (GroupPrincipal Group = GroupPrincipal.FindByIdentity(mainContext, groupName))
            {
                //var tmp2 = GetGroupMemberList(Group, true);

                using (var listMember = Group.GetMembers(true))// Members.ToList())
                {
                    var db = new IncentiviEntities();
                    List<UserPrincipal> tmp = new List<UserPrincipal>();
                    tmp.AddRange(listMember.Cast<UserPrincipal>());
                    var elencoMatr = tmp.Select(x => x.SamAccountName.StartsWith("P") ? x.SamAccountName.Substring(1) : "").Where(x => x != "");
                    var elencoSint = db.SINTESI1.Where(x => elencoMatr.Contains(x.COD_MATLIBROMAT))
                                        .Select(x => new
                                        {
                                            x.COD_MATLIBROMAT,
                                            x.DES_SEDE,
                                            x.DES_SERVIZIO
                                        }).ToList();

                    foreach (UserPrincipal item in tmp)
                    {
                        var sint = elencoSint.FirstOrDefault(x => x.COD_MATLIBROMAT == item.SamAccountName.Substring(1));

                        gruppoAd.Persone.Add(new UserAD()
                        {
                            Matricola = item.Name,
                            Nominativo = item.DisplayName,
                            Sede = sint != null ? sint.DES_SEDE : "",
                            Servizio = sint != null ? sint.DES_SERVIZIO : ""
                        });
                    }
                }
            }

            return result;
        }

        public static List<UserAD> GetListDip(UserAdSearch model)
        {
            List<UserAD> result = new List<UserAD>();

            var db = new IncentiviEntities();

            var tmp = db.SINTESI1.Where(x => x.DTA_FINE_CR > DateTime.Today);

            if (!String.IsNullOrWhiteSpace(model.Matricola))
                tmp = tmp.Where(x => model.Matricola.Contains(x.COD_MATLIBROMAT));

            if (!String.IsNullOrWhiteSpace(model.Nominativo))
            {
                string UpNom = model.Nominativo.ToUpper();
                tmp = tmp.Where(x => (x.DES_COGNOMEPERS + " " + x.DES_NOMEPERS).StartsWith(UpNom) || (x.DES_NOMEPERS + " " + x.DES_COGNOMEPERS).StartsWith(UpNom));
            }

            if (!String.IsNullOrWhiteSpace(model.Sede))
            {
                string upSede = model.Sede.ToUpper();
                tmp = tmp.Where(x => x.DES_SEDE.Contains(upSede));
            }

            if (!String.IsNullOrWhiteSpace(model.Servizio))
            {
                string upServ = model.Servizio.ToUpper();
                tmp = tmp.Where(x => x.DES_SERVIZIO.Contains(upServ));
            }

            var tmpResult = tmp.ToList();
            if (tmpResult.Any())
                result.AddRange(tmpResult.Select(x => new UserAD()
                {
                    Matricola = "P" + x.COD_MATLIBROMAT,
                    Nominativo = x.DES_COGNOMEPERS.TitleCase() + " " + x.DES_NOMEPERS.TitleCase(),
                    Sede = x.DES_SEDE,
                    Servizio = x.DES_SERVIZIO
                }));

            return result;
        }

        public static bool AggiungiDipendenti(string nomeGruppo, string elencoMatr, out string errorMsg)
        {
            bool result = false;
            errorMsg = "";
            var account = CommonHelper.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio_AD);
            var matricole = elencoMatr.Split(',');

            using (PrincipalContext mainContext = new PrincipalContext(ContextType.Domain, "RAI", account[0], account[1]))
            {

                using (GroupPrincipal Group = GroupPrincipal.FindByIdentity(mainContext, nomeGruppo))
                {
                    var listMember = Group.Members.ToList();
                    bool hasError = false;
                    foreach (var item in matricole.Where(x => !String.IsNullOrWhiteSpace(x) && !listMember.Any(y => y.SamAccountName == x)))
                    {
                        try
                        {
                            UserPrincipal user = UserPrincipal.FindByIdentity(mainContext, IdentityType.SamAccountName, item);
                            Group.Members.Add(user);
                            Group.Save(mainContext);
                        }
                        catch (Exception ex)
                        {
                            hasError = true;
                            errorMsg = ex.Message;
                            break;
                        }
                    }

                    result = !hasError;
                }
            }

            return result;
        }

        public static bool RimuoviDipendenti(string nomeGruppo, string elencoMatr, out string errorMsg)
        {
            bool result = false;
            errorMsg = "";
            var account = CommonHelper.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio_AD);
            var matricole = elencoMatr.Split(',');

            using (PrincipalContext mainContext = new PrincipalContext(ContextType.Domain, "RAI", account[0], account[1]))
            {

                using (GroupPrincipal Group = GroupPrincipal.FindByIdentity(mainContext, nomeGruppo))
                {
                    bool hasError = false;
                    var listMember = Group.Members.ToList();
                    foreach (var item in listMember.Where(x => matricole.Contains(x.SamAccountName)))
                    {
                        try
                        {
                            Group.Members.Remove(item);
                            Group.Save(mainContext);
                        }
                        catch (Exception ex)
                        {
                            hasError = true;
                            errorMsg = ex.Message;
                            break;
                        }

                    }

                    result = !hasError;
                }
            }

            return result;
        }
    }
}