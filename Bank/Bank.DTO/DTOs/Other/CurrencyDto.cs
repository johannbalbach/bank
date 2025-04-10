using System.Xml.Serialization;

namespace Bank.DTO.DTOs.Other
{
    [XmlRoot(ElementName = "Envelope", Namespace = "http://www.w3.org/2003/05/soap-envelope")]
    public class CurrencyDtoFromBankRf
    {
        [XmlElement(ElementName = "Body", Namespace = "http://www.w3.org/2003/05/soap-envelope")]
        public Body Body { get; set; }
    }

    public class Body
    {
        [XmlElement(ElementName = "GetCursOnDateXMLResponse", Namespace = "http://web.cbr.ru/")]
        public GetCursOnDateXMLResponse GetCursOnDateXMLResponse { get; set; }
    }

    public class GetCursOnDateXMLResponse
    {
        [XmlElement(ElementName = "GetCursOnDateXMLResult", Namespace = "http://web.cbr.ru/")]
        public GetCursOnDateXMLResult GetCursOnDateXMLResult { get; set; }
    }

    public class GetCursOnDateXMLResult
    {
        [XmlElement(ElementName = "ValuteData", Namespace = "")]
        public ValuteData ValuteData { get; set; }
    }

    public class ValuteData
    {
        [XmlAttribute(AttributeName = "OnDate")]
        public string OnDate { get; set; }

        [XmlElement(ElementName = "ValuteCursOnDate")]
        public List<ValuteCursOnDate> ValuteCursOnDate { get; set; }
    }

    public class ValuteCursOnDate
    {
        /// <summary>
        /// Название
        /// </summary>
        [XmlElement(ElementName = "Vname")]
        public string Vname { get; set; }
        /// <summary>
        /// Номинал
        /// </summary>
        [XmlElement(ElementName = "Vnom")]
        public int Vnom { get; set; }
        /// <summary>
        /// Курс
        /// </summary>
        [XmlElement(ElementName = "Vcurs")]
        public decimal Vcurs { get; set; }
        /// <summary>
        /// ISO Цифровой код валюты
        /// </summary>
        [XmlElement(ElementName = "Vcode")]
        public int Vcode { get; set; }
        /// <summary>
        /// ISO Символьный код валюты
        /// </summary>
        [XmlElement(ElementName = "VchCode")]
        public string VchCode { get; set; }
        /// <summary>
        /// Курс за 1 единицу валюты
        /// </summary>
        [XmlElement(ElementName = "VunitRate")]
        public decimal VunitRate { get; set; }
    }
}
