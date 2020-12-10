using System.Xml.Serialization;

namespace ProductShop.Dtos.Export.UserCountDTO
{
    [XmlType("Product")]
    public class ExportProductDTO
    {
        [XmlElement("name")]
        public string Name { get; set; }

        [XmlElement("price")]
        public decimal Price { get; set; }
    }
}
