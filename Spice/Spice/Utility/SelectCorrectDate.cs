using Spice.Extensions;
using Spice.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Spice.Utility
{
    public class SelectCorrectDate : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var order = validationContext.ObjectInstance as OrderHeader;

            if(order.PickUpDate.Date.Day != DateTime.Today.Date.Day)
            {
                return new ValidationResult("Date is not valid");
            }
            if(order.PickUpDate.Date == null)
            {
                return new ValidationResult("Enter Date");
            }
            if(order.PickUpDate.Date.Day> DateTime.Today.Date.Day + 6)
            {
                return new ValidationResult("You can place order within one week only");
            }
            else
            {
                return ValidationResult.Success;
            }

        }
    }
}
