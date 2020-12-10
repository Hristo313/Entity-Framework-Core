using System.Xml.Serialization;

namespace ProductShop.Dtos.Export.UserCountDTO
{

    public class ExportProductCountDTO
    {
        [XmlElement("count")]
        public int Count { get; set; }

        [XmlArray("products")]
        public ExportProductDTO[] Products { get; set; }
    }
}
