using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Xml.Serialization;
using VaporStore.Data.Models.Enums;

namespace VaporStore.DataProcessor.Dto.Import
{
    [XmlType("Purchase")]
 public class ImportPurchaseDto
    {
        [XmlAttribute("title")]
        public string Title { get; set; }
        [XmlElement("Type")]
        [Required]
        [Range(0,1)]
        public PurchaseType Type { get; set; }
        [XmlElement("Key")]
        [Required]
        [RegularExpression(@"^([A-Z1-9]{4})-([A-Z1-9]{4})-([A-Z1-9]{4})$")]
        public string Key { get; set; }
        [XmlElement("Card")]
        [Required]
        [RegularExpression(@"^(\d{4}) (\d{4}) (\d{4}) (\d{4})$")]
        public string CardNumber { get; set; }
        [XmlElement("Date")]
        [Required]
        public string Date { get; set; }
    }
}
