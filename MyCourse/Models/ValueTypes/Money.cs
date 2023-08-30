using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyCourse.Models.Enums;

namespace MyCourse.Models.ValueTypes
{
    public record Money
    {
        public Money() : this(Currency.EUR, 0.00m)
        {
        }
        public Money(Currency currency, decimal amount)
        {
            Amount = amount;
            Currency = currency;
        }
        private decimal amount = 0;
        public decimal Amount
        { 
            get
            {
                return amount;
            }
            set
            {
                if (value < 0) {
                    throw new InvalidOperationException("The amount cannot be negative");
                }
                amount = value;
            }
        }
        public Currency Currency
        {
            get; init;
        }

        ///<summary>
        ////CIAOOO
        ///</summary>
        public override string ToString()
        {
            return $"{Currency} {Amount:#.00}";
        }
    }
}