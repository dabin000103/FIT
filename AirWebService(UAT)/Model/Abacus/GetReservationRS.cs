using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;

namespace AirWebService.Model.Abacus
{
    [XmlRoot(ElementName = "GetReservationRS", Namespace = "http://webservices.sabre.com/pnrbuilder/v1_19")]
    [DataContract(Name = "GetReservationRS")]
    public class GetReservationRS
    {
        
        [XmlElement(ElementName = "Reservation")]
        public Reservation Reservation { get; set; }

        [XmlElement(ElementName = "PriceQuote", Namespace = "http://services.sabre.com/res/or/v1_14")]
        public PriceQuoteRoot PriceQuote { get; set; }
    }

    [XmlRoot(ElementName = "BookingDetails")]
    public class BookingDetails
    {
        [XmlElement(ElementName = "RecordLocator")]
        public string RecordLocator { get; set; }
        [XmlElement(ElementName = "CreationTimestamp")]
        public string CreationTimestamp { get; set; }
        [XmlElement(ElementName = "SystemCreationTimestamp")]
        public string SystemCreationTimestamp { get; set; }
        [XmlElement(ElementName = "CreationAgentID")]
        public string CreationAgentID { get; set; }
        [XmlElement(ElementName = "UpdateTimestamp")]
        public string UpdateTimestamp { get; set; }
        [XmlElement(ElementName = "PNRSequence")]
        public string PNRSequence { get; set; }
        [XmlElement(ElementName = "DivideSplitDetails")]
        public string DivideSplitDetails { get; set; }
        [XmlElement(ElementName = "EstimatedPurgeTimestamp")]
        public string EstimatedPurgeTimestamp { get; set; }
        [XmlElement(ElementName = "UpdateToken")]
        public string UpdateToken { get; set; }
    }

    public class PriceQuoteRoot
    {
        [XmlElement(ElementName = "PriceQuoteInfo", Namespace = "http://www.sabre.com/ns/Ticketing/pqs/1.0")]
        public PriceQuoteInfo PriceQuoteInfo { get; set; }
    }

    [XmlRoot(ElementName = "Source")]
    public class Source
    {
        [XmlAttribute(AttributeName = "BookingSource")]
        public string BookingSource { get; set; }
        [XmlAttribute(AttributeName = "AgentSine")]
        public string AgentSine { get; set; }
        [XmlAttribute(AttributeName = "PseudoCityCode")]
        public string PseudoCityCode { get; set; }
        [XmlAttribute(AttributeName = "ISOCountry")]
        public string ISOCountry { get; set; }
        [XmlAttribute(AttributeName = "AgentDutyCode")]
        public string AgentDutyCode { get; set; }
        [XmlAttribute(AttributeName = "AirlineVendorID")]
        public string AirlineVendorID { get; set; }
        [XmlAttribute(AttributeName = "HomePseudoCityCode")]
        public string HomePseudoCityCode { get; set; }
        [XmlAttribute(AttributeName = "PrimeHostID")]
        public string PrimeHostID { get; set; }
    }

    [XmlRoot(ElementName = "POS")]
    public class POS
    {
        [XmlElement(ElementName = "Source")]
        public Source Source { get; set; }
        [XmlAttribute(AttributeName = "AirExtras")]
        public string AirExtras { get; set; }
        [XmlAttribute(AttributeName = "InhibitCode")]
        public string InhibitCode { get; set; }
    }

    [XmlRoot(ElementName = "PassengerReservation")]
    public class PassengerReservation
    {
        [XmlElement(ElementName = "Passengers")]
        public string Passengers { get; set; }
        [XmlElement(ElementName = "Segments")]
        public string Segments { get; set; }
        [XmlElement(ElementName = "TicketingInfo")]
        public string TicketingInfo { get; set; }
        [XmlElement(ElementName = "ItineraryPricing")]
        public string ItineraryPricing { get; set; }
    }

    [XmlRoot(ElementName = "Reservation")]
    public class Reservation
    {
        [XmlElement(ElementName = "BookingDetails")]
        public BookingDetails BookingDetails { get; set; }
        [XmlElement(ElementName = "POS")]
        public POS POS { get; set; }
        [XmlElement(ElementName = "PassengerReservation")]
        public PassengerReservation PassengerReservation { get; set; }
        [XmlElement(ElementName = "ReceivedFrom")]
        public string ReceivedFrom { get; set; }
        [XmlElement(ElementName = "EmailAddresses")]
        public string EmailAddresses { get; set; }
        [XmlAttribute(AttributeName = "numberInParty")]
        public string NumberInParty { get; set; }
        [XmlAttribute(AttributeName = "numberOfInfants")]
        public string NumberOfInfants { get; set; }
        [XmlAttribute(AttributeName = "NumberInSegment")]
        public string NumberInSegment { get; set; }
        [XmlAttribute(AttributeName = "updateToken")]
        public string UpdateToken { get; set; }
    }

    [XmlRoot(ElementName = "Passenger")]
    public class Passenger
    {
        [XmlAttribute(AttributeName = "passengerTypeCount")]
        public string PassengerTypeCount { get; set; }
        [XmlAttribute(AttributeName = "requestedType")]
        public string RequestedType { get; set; }
        [XmlAttribute(AttributeName = "type")]
        public string Type { get; set; }
    }

    [XmlRoot(ElementName = "Total")]
    public class Total
    {
        [XmlAttribute(AttributeName = "currencyCode")]
        public string CurrencyCode { get; set; }
        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "Amounts")]
    public class Amounts
    {
        [XmlElement(ElementName = "Total")]
        public Total Total { get; set; }
    }

    public class PriceQuote
    {
        [XmlElement(ElementName = "Passenger")]
        public Passenger Passenger { get; set; }
        [XmlElement(ElementName = "ItineraryType")]
        public string ItineraryType { get; set; }
        [XmlElement(ElementName = "ValidatingCarrier")]
        public string ValidatingCarrier { get; set; }
        [XmlElement(ElementName = "Amounts")]
        public Amounts Amounts { get; set; }
        [XmlElement(ElementName = "LocalCreateDateTime")]
        public string LocalCreateDateTime { get; set; }
        [XmlAttribute(AttributeName = "latestPQFlag")]
        public string LatestPQFlag { get; set; }
        [XmlAttribute(AttributeName = "number")]
        public string Number { get; set; }
        [XmlAttribute(AttributeName = "pricingStatus")]
        public string PricingStatus { get; set; }
        [XmlAttribute(AttributeName = "pricingType")]
        public string PricingType { get; set; }
        [XmlAttribute(AttributeName = "status")]
        public string Status { get; set; }
        [XmlAttribute(AttributeName = "type")]
        public string Type { get; set; }
    }

    [XmlRoot(ElementName = "NameAssociation")]
    public class NameAssociation
    {
        [XmlElement(ElementName = "PriceQuote")]
        public PriceQuote PriceQuote { get; set; }
        [XmlAttribute(AttributeName = "firstName")]
        public string FirstName { get; set; }
        [XmlAttribute(AttributeName = "lastName")]
        public string LastName { get; set; }
        [XmlAttribute(AttributeName = "nameId")]
        public string NameId { get; set; }
        [XmlAttribute(AttributeName = "nameNumber")]
        public string NameNumber { get; set; }
    }

    [XmlRoot(ElementName = "Summary")]
    public class Summary
    {
        [XmlElement(ElementName = "NameAssociation")]
        public NameAssociation NameAssociation { get; set; }
    }

    [XmlRoot(ElementName = "AgentInfo")]
    public class AgentInfo
    {
        [XmlElement(ElementName = "HomeLocation")]
        public string HomeLocation { get; set; }
        [XmlElement(ElementName = "WorkLocation")]
        public string WorkLocation { get; set; }
        [XmlAttribute(AttributeName = "duty")]
        public string Duty { get; set; }
        [XmlAttribute(AttributeName = "sine")]
        public string Sine { get; set; }
    }

    [XmlRoot(ElementName = "TransactionInfo")]
    public class TransactionInfo
    {
        [XmlElement(ElementName = "CreateDateTime")]
        public string CreateDateTime { get; set; }
        [XmlElement(ElementName = "UpdateDateTime")]
        public string UpdateDateTime { get; set; }
        [XmlElement(ElementName = "LastDateToPurchase")]
        public string LastDateToPurchase { get; set; }
        [XmlElement(ElementName = "LocalCreateDateTime")]
        public string LocalCreateDateTime { get; set; }
        [XmlElement(ElementName = "LocalUpdateDateTime")]
        public string LocalUpdateDateTime { get; set; }
        [XmlElement(ElementName = "LocalDateTime")]
        public string LocalDateTime { get; set; }
        [XmlElement(ElementName = "InputEntry")]
        public string InputEntry { get; set; }
    }

    [XmlRoot(ElementName = "NameAssociationInfo")]
    public class NameAssociationInfo
    {
        [XmlAttribute(AttributeName = "firstName")]
        public string FirstName { get; set; }
        [XmlAttribute(AttributeName = "lastName")]
        public string LastName { get; set; }
        [XmlAttribute(AttributeName = "nameId")]
        public string NameId { get; set; }
        [XmlAttribute(AttributeName = "nameNumber")]
        public string NameNumber { get; set; }
    }

    [XmlRoot(ElementName = "MarketingFlight")]
    public class MarketingFlight
    {
        [XmlAttribute(AttributeName = "number")]
        public string Number { get; set; }
        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "CityCode")]
    public class CityCode
    {
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "Departure")]
    public class Departure
    {
        [XmlElement(ElementName = "DateTime")]
        public string DateTime { get; set; }
        [XmlElement(ElementName = "CityCode")]
        public CityCode CityCode { get; set; }
    }

    [XmlRoot(ElementName = "Arrival")]
    public class Arrival
    {
        [XmlElement(ElementName = "DateTime")]
        public string DateTime { get; set; }
        [XmlElement(ElementName = "CityCode")]
        public CityCode CityCode { get; set; }
    }

    [XmlRoot(ElementName = "Flight")]
    public class Flight
    {
        [XmlElement(ElementName = "MarketingFlight")]
        public MarketingFlight MarketingFlight { get; set; }
        [XmlElement(ElementName = "ClassOfService")]
        public string ClassOfService { get; set; }
        [XmlElement(ElementName = "Departure")]
        public Departure Departure { get; set; }
        [XmlElement(ElementName = "Arrival")]
        public Arrival Arrival { get; set; }
        [XmlAttribute(AttributeName = "connectionIndicator")]
        public string ConnectionIndicator { get; set; }
    }

    [XmlRoot(ElementName = "Baggage")]
    public class Baggage
    {
        [XmlAttribute(AttributeName = "allowance")]
        public string Allowance { get; set; }
        [XmlAttribute(AttributeName = "type")]
        public string Type { get; set; }
    }

    [XmlRoot(ElementName = "SegmentInfo")]
    public class SegmentInfo
    {
        [XmlElement(ElementName = "Flight")]
        public Flight Flight { get; set; }
        [XmlElement(ElementName = "FareBasis")]
        public string FareBasis { get; set; }
        [XmlElement(ElementName = "NotValidBefore")]
        public string NotValidBefore { get; set; }
        [XmlElement(ElementName = "NotValidAfter")]
        public string NotValidAfter { get; set; }
        [XmlElement(ElementName = "Baggage")]
        public Baggage Baggage { get; set; }
        [XmlAttribute(AttributeName = "number")]
        public string Number { get; set; }
        [XmlAttribute(AttributeName = "segmentStatus")]
        public string SegmentStatus { get; set; }
        [XmlAttribute(AttributeName = "type")]
        public string Type { get; set; }
    }

    [XmlRoot(ElementName = "FareIndicators")]
    public class FareIndicators
    {
        [XmlAttribute(AttributeName = "privateFareType")]
        public string PrivateFareType { get; set; }
    }

    [XmlRoot(ElementName = "BaseFare")]
    public class BaseFare
    {
        [XmlAttribute(AttributeName = "currencyCode")]
        public string CurrencyCode { get; set; }
        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "TotalTax")]
    public class TotalTax
    {
        [XmlAttribute(AttributeName = "currencyCode")]
        public string CurrencyCode { get; set; }
        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "TotalFare")]
    public class TotalFare
    {
        [XmlAttribute(AttributeName = "currencyCode")]
        public string CurrencyCode { get; set; }
        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "Amount")]
    public class Amount
    {
        [XmlAttribute(AttributeName = "currencyCode")]
        public string CurrencyCode { get; set; }
        [XmlText]
        public string Text { get; set; }
        [XmlAttribute(AttributeName = "decimalPlace")]
        public string DecimalPlace { get; set; }
    }

    [XmlRoot(ElementName = "CombinedTax")]
    public class CombinedTax
    {
        [XmlElement(ElementName = "Amount")]
        public Amount Amount { get; set; }
        [XmlAttribute(AttributeName = "code")]
        public string Code { get; set; }
    }

    [XmlRoot(ElementName = "Tax")]
    public class Tax
    {
        [XmlElement(ElementName = "Amount")]
        public Amount Amount { get; set; }
        [XmlAttribute(AttributeName = "code")]
        public string Code { get; set; }
    }

    [XmlRoot(ElementName = "TaxInfo")]
    public class TaxInfo
    {
        [XmlElement(ElementName = "CombinedTax")]
        public List<CombinedTax> CombinedTax { get; set; }
        [XmlElement(ElementName = "Tax")]
        public List<Tax> Tax { get; set; }
    }

    [XmlRoot(ElementName = "FlightSegmentNumbers")]
    public class FlightSegmentNumbers
    {
        [XmlElement(ElementName = "SegmentNumber")]
        public List<string> SegmentNumber { get; set; }
    }

    [XmlRoot(ElementName = "FareDirectionality")]
    public class FareDirectionality
    {
        [XmlAttribute(AttributeName = "roundTrip")]
        public string RoundTrip { get; set; }
        [XmlAttribute(AttributeName = "inbound")]
        public string Inbound { get; set; }
    }

    [XmlRoot(ElementName = "FareComponent")]
    public class FareComponent
    {
        [XmlElement(ElementName = "FlightSegmentNumbers")]
        public FlightSegmentNumbers FlightSegmentNumbers { get; set; }
        [XmlElement(ElementName = "FareDirectionality")]
        public FareDirectionality FareDirectionality { get; set; }
        [XmlElement(ElementName = "Departure")]
        public Departure Departure { get; set; }
        [XmlElement(ElementName = "Arrival")]
        public Arrival Arrival { get; set; }
        [XmlElement(ElementName = "Amount")]
        public Amount Amount { get; set; }
        [XmlElement(ElementName = "GoverningCarrier")]
        public string GoverningCarrier { get; set; }
        [XmlAttribute(AttributeName = "fareBasisCode")]
        public string FareBasisCode { get; set; }
        [XmlAttribute(AttributeName = "number")]
        public string Number { get; set; }
    }

    [XmlRoot(ElementName = "FareInfo")]
    public class FareInfo
    {
        [XmlElement(ElementName = "FareIndicators")]
        public FareIndicators FareIndicators { get; set; }
        [XmlElement(ElementName = "BaseFare")]
        public BaseFare BaseFare { get; set; }
        [XmlElement(ElementName = "TotalTax")]
        public TotalTax TotalTax { get; set; }
        [XmlElement(ElementName = "TotalFare")]
        public TotalFare TotalFare { get; set; }
        [XmlElement(ElementName = "TaxInfo")]
        public TaxInfo TaxInfo { get; set; }
        [XmlElement(ElementName = "FareCalculation")]
        public string FareCalculation { get; set; }
        [XmlElement(ElementName = "FareComponent")]
        public List<FareComponent> FareComponent { get; set; }
        [XmlAttribute(AttributeName = "source")]
        public string Source { get; set; }
    }

    [XmlRoot(ElementName = "FeeInfo")]
    public class FeeInfo
    {
        [XmlElement(ElementName = "OBFee")]
        public List<OBFee> OBFee { get; set; }
    }    

    [XmlRoot(ElementName = "OBFee")]
    public class OBFee
    {
        [XmlElement(ElementName = "Amount")]
        public OBFeeAmount OBFeeAmount { get; set; }
        [XmlElement(ElementName = "Total")]
        public OBFeeTotal OBFeeTotal { get; set; }
        [XmlElement(ElementName = "Description")]
        public string Description { get; set; }
        [XmlElement(ElementName = "BankIdentificationNumber")]
        public string BankIdentificationNumber { get; set; }
        [XmlAttribute(AttributeName = "code")]
        public string Code { get; set; }
        [XmlAttribute(AttributeName = "noChargeIndicator")]
        public string noChargeIndicator { get; set; }
        [XmlAttribute(AttributeName = "type")]
        public string Type { get; set; }
    }
    public class OBFeeAmount
    {
        [XmlAttribute(AttributeName = "currencyCode")]
        public string CurrencyCode { get; set; }
        //[XmlAttribute(AttributeName = "decimalPlace")]
        //public string DecimalPlace { get; set; }
        [XmlText]
        public string Text { get; set; }
    }
    public class OBFeeTotal
    {
        [XmlAttribute(AttributeName = "currencyCode")]
        public string CurrencyCode { get; set; }
        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "MiscellaneousInfo")]
    public class MiscellaneousInfo
    {
        [XmlElement(ElementName = "ValidatingCarrier")]
        public string ValidatingCarrier { get; set; }
        [XmlElement(ElementName = "TourNumber")]
        public TourNumber TourNumber { get; set; }
        [XmlElement(ElementName = "ItineraryType")]
        public string ItineraryType { get; set; }
    }
    public class TourNumber
    {
        [XmlAttribute(AttributeName = "code")]
        public string Code { get; set; }
        //[XmlText]
        //public string Text { get; set; }
    }

    [XmlRoot(ElementName = "Message")]
    public class Message
    {
        [XmlAttribute(AttributeName = "number")]
        public string Number { get; set; }
        [XmlAttribute(AttributeName = "type")]
        public string Type { get; set; }
        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "Remarks")]
    public class Remarks
    {
        [XmlAttribute(AttributeName = "type")]
        public string Type { get; set; }
        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "MessageInfo")]
    public class MessageInfo
    {
        [XmlElement(ElementName = "Message")]
        public List<Message> Message { get; set; }
        [XmlElement(ElementName = "Remarks")]
        public List<Remarks> Remarks { get; set; }
        [XmlElement(ElementName = "PricingParameters")]
        public string PricingParameters { get; set; }
    }

    [XmlRoot(ElementName = "HistoryInfo")]
    public class HistoryInfo
    {
        [XmlElement(ElementName = "AgentInfo")]
        public AgentInfo AgentInfo { get; set; }
        [XmlElement(ElementName = "TransactionInfo")]
        public TransactionInfo TransactionInfo { get; set; }
    }

    [XmlRoot(ElementName = "Details")]
    public class Details
    {
        [XmlElement(ElementName = "AgentInfo")]
        public AgentInfo AgentInfo { get; set; }
        [XmlElement(ElementName = "TransactionInfo")]
        public TransactionInfo TransactionInfo { get; set; }
        [XmlElement(ElementName = "NameAssociationInfo")]
        public NameAssociationInfo NameAssociationInfo { get; set; }
        [XmlElement(ElementName = "SegmentInfo")]
        public List<SegmentInfo> SegmentInfo { get; set; }
        [XmlElement(ElementName = "FareInfo")]
        public FareInfo FareInfo { get; set; }
        [XmlElement(ElementName = "FeeInfo")]
        public FeeInfo FeeInfo { get; set; }
        [XmlElement(ElementName = "MiscellaneousInfo")]
        public MiscellaneousInfo MiscellaneousInfo { get; set; }
        [XmlElement(ElementName = "MessageInfo")]
        public MessageInfo MessageInfo { get; set; }
        [XmlElement(ElementName = "HistoryInfo")]
        public HistoryInfo HistoryInfo { get; set; }
        [XmlAttribute(AttributeName = "number")]
        public string Number { get; set; }
        [XmlAttribute(AttributeName = "passengerType")]
        public string PassengerType { get; set; }
        [XmlAttribute(AttributeName = "pricingStatus")]
        public string PricingStatus { get; set; }
        [XmlAttribute(AttributeName = "pricingType")]
        public string PricingType { get; set; }
        [XmlAttribute(AttributeName = "status")]
        public string Status { get; set; }
        [XmlAttribute(AttributeName = "type")]
        public string Type { get; set; }
    }

    //[XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.sabre.com/ns/Ticketing/pqs/1.0")]
    //[XmlRootAttribute(Namespace = "http://www.sabre.com/ns/Ticketing/pqs/1.0", IsNullable = false)]
    public class PriceQuoteInfo
    {
        [XmlElement(ElementName = "Reservation")]
        public Reservation PriceReservation { get; set; }
        [XmlElement(ElementName = "Summary")]
        public Summary Summary { get; set; }
        [XmlElement(ElementName = "Details")]
        public Details Details { get; set; }
    }

    
    
}