using System.ComponentModel.DataAnnotations;

namespace EmailAPI.DTOs
{

    public class Payment
    {
        [Required]
        public string TransactionReference { get; set; }

        [Required]
        public string TruckNumber { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public int Amount { get; set; }
    }
}

