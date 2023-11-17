using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace myRaiCommonModel.cvModels
{
    public class VotoScalaCheckAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var model = (cvModel.Studies)validationContext.ObjectInstance;
            int voto, scala;

            try
            {
                scala = Convert.ToInt32(value.ToString());
            }
            catch (Exception exc)
            {
                scala = 0;
            }
            try
            {
                voto = Convert.ToInt32(model._voto);
            }
            catch (Exception exc)
            {
                voto = 0;
            }

            if ((voto > scala) || ((scala == 0) ^ (voto == 0)))
            {
                return new ValidationResult("scala");
            }
            else
            {
                return ValidationResult.Success;
            }

        }
    }

    public class InizioFineDataCheckAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var model = (cvModel.Studies)validationContext.ObjectInstance;
            string tmp_inizio, tmp_fine;

            try
            {
                tmp_inizio = (model._dataInizio.Length == 4) ? "01/01/" + model._dataInizio : model._dataInizio;
            }
            catch (Exception ex)
            {
                tmp_inizio = null;
            }

            try
            {
                tmp_fine = (value.ToString().Length == 4) ? "01/01/" + value.ToString() : value.ToString();
            }
            catch (Exception ex)
            {
                tmp_fine = null;
            }
            DateTime? dataInizio, dataFine;
            try
            {
                dataFine = DateTime.Parse(tmp_fine);
            }
            catch (Exception exc)
            {
                return new ValidationResult("");
            }

            try
            {
                dataInizio = DateTime.Parse(tmp_inizio);
            }
            catch (Exception exc)
            {
                return ValidationResult.Success;
            }

            if (dataInizio < dataFine)
            {
                return ValidationResult.Success;
            }
            else
            {
                return new ValidationResult("datafine");
            }
        }
    }
}