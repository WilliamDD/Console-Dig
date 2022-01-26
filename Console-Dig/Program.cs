// See https://aka.ms/new-console-template for more information
using CommandLine;
using DnsClient;
using DnsClient.Protocol;
using DnsClient.Protocol.Options;
using System.Net;

IPEndPoint dnsServer = new IPEndPoint(IPAddress.Parse("8.8.4.4"), 57);
myQueryClass queryClass = myQueryClass.IN;
myQueryType queryType = myQueryType.ANY;
bool recursion = false;
bool resolverCache = false;
bool tcpOnly = false;
bool ConvertIpAddressToArpaWhenUsingPTR = true;
bool auditTrail = false;
string query = "";

Parser.Default.ParseArguments<Options>(args)
    .WithParsed<Options>(o =>
    {
        dnsServer = new IPEndPoint(IPAddress.Parse(o.Server), 53);
        queryClass = o.Class;
        queryType = o.Type;
        recursion = o.Recursion;
        resolverCache = false;
        tcpOnly = o.tcp;
        query = o.Name;
        ConvertIpAddressToArpaWhenUsingPTR = true;
        auditTrail = o.AuditTrail;
    });

var clientOptions = new LookupClientOptions(dnsServer);
clientOptions.Recursion = recursion;
clientOptions.UseCache = resolverCache;
clientOptions.UseTcpOnly = tcpOnly;
clientOptions.EnableAuditTrail = auditTrail;

var lookupClient = new LookupClient(clientOptions);
var result = await lookupClient.QueryAsync(query, (QueryType)queryType, (QueryClass)queryClass);
Console.WriteLine(result.Header);
Console.WriteLine();

Console.WriteLine(";; OPT PSEUDOSECTION:");
foreach (OptRecord additional in result.Additionals)
{
    Console.WriteLine($"; EDNS: version: {additional.Version}, flags:; UDP: {additional.UdpSize}; code: {additional.ResponseCodeEx}; dnsSec: {additional.IsDnsSecOk}");
}

Console.WriteLine();
Console.WriteLine(";; Question section");
foreach (var question in result.Questions)
{
    Console.WriteLine(question);
}
Console.WriteLine();
Console.WriteLine(";; Answer section");
foreach (var answer in result.Answers)
{
    Console.WriteLine(answer);
}
//Console.WriteLine();
//Console.WriteLine(";; Audit trail section");
//Console.WriteLine(result.AuditTrail);

public class Options
{
    [Option('s', "server", Required = true, HelpText = "is the name or IP address of the name server to query. This can be an IPv4 address in dotted-decimal notation or an IPv6 address in colon-delimited notation.")]
    public string Server { get; set; }

    [Option('n', "name", Required = true, HelpText = "is the name of the resource record that is to be looked up.")]
    public string Name { get; set; }

    [Option('t', "type", Default = myQueryType.ANY, Required = true, HelpText = "indicates what type of query is required - ANY, A, MX, SIG, etc. type can be any valid query type. If no type argument is supplied, dig will perform a lookup for an A record.")]
    public myQueryType Type { get; set; }

    [Option('c', "class", Default = myQueryClass.IN, Required = false, HelpText = "class is any valid class, such as HS for Hesiod records or CH for Chaosnet records")]
    public myQueryClass Class { get; set; }

    [Option('r', "recursive",Default = true,  Required = false, HelpText = "Gets or sets a flag indicating whether DNS queries should instruct the DNS server to do recursive lookups, or not. Default is True.")]
    public bool Recursion { get; set; }

    [Option("tcp", Default = false, Required = false, HelpText = "Gets or sets a flag indicating whether UDP should not be used at all. Default is False. Enable this only if UDP cannot be used because of your firewall rules for example. Also, zone transfers (QueryType.AXFR) must use TCP only.")]
    public bool tcp { get; set; }

    [Option("resolverCache", Default = true, Required = false, HelpText = "Gets or sets a flag indicating whether DNS queries should use response caching or not. The cache duration is calculated by the resource record of the response. Usually, the lowest TTL is used. Default is True.")]
    public bool resolverCache { get; set; }

    [Option("audittrail", Default = false, Required = false, HelpText = "Gets or sets a flag indicating whether each DnsClient.IDnsQueryResponse will contain a full documentation of the response(s). Default is False.")]
    public bool AuditTrail { get; set; }
}

[Flags]
public enum myQueryClass : short
{
    //
    // Summary:
    //     The Internet.
    IN = 1,
    //
    // Summary:
    //     The CSNET class (Obsolete - used only for examples in some obsolete RFCs).
    CS,
    //
    // Summary:
    //     The CHAOS class.
    CH,
    //
    // Summary:
    //     Hesiod [Dyer 87].
    HS
}

[Flags]
public enum myQueryType
{
    //
    // Summary:
    //     A host address.
    A = 1,
    //
    // Summary:
    //     An authoritative name server.
    NS = 2,
    //
    // Summary:
    //     A mail destination (OBSOLETE - use MX).
    [Obsolete("Use MX")]
    MD = 3,
    //
    // Summary:
    //     A mail forwarder (OBSOLETE - use MX).
    [Obsolete("Use MX")]
    MF = 4,
    //
    // Summary:
    //     The canonical name for an alias.
    CNAME = 5,
    //
    // Summary:
    //     Marks the start of a zone of authority.
    SOA = 6,
    //
    // Summary:
    //     A mailbox domain name (EXPERIMENTAL).
    MB = 7,
    //
    // Summary:
    //     A mail group member (EXPERIMENTAL).
    MG = 8,
    //
    // Summary:
    //     A mailbox rename domain name (EXPERIMENTAL).
    MR = 9,
    //
    // Summary:
    //     A Null resource record (EXPERIMENTAL).
    NULL = 10,
    //
    // Summary:
    //     A well known service description.
    WKS = 11,
    //
    // Summary:
    //     A domain name pointer.
    PTR = 12,
    //
    // Summary:
    //     Host information.
    HINFO = 13,
    //
    // Summary:
    //     Mailbox or mail list information.
    MINFO = 14,
    //
    // Summary:
    //     Mail exchange.
    MX = 0xF,
    //
    // Summary:
    //     Text resources.
    TXT = 0x10,
    //
    // Summary:
    //     Responsible Person.
    RP = 17,
    //
    // Summary:
    //     AFS Data Base location.
    AFSDB = 18,
    //
    // Summary:
    //     An IPv6 host address.
    AAAA = 28,
    //
    // Summary:
    //     A resource record which specifies the location of the server(s) for a specific
    //     protocol and domain.
    SRV = 33,
    //
    // Summary:
    //     The Naming Authority Pointer rfc2915
    NAPTR = 35,
    //
    // Summary:
    //     DS rfc4034
    DS = 43,
    //
    // Summary:
    //     RRSIG rfc3755.
    RRSIG = 46,
    //
    // Summary:
    //     NSEC rfc4034.
    NSEC = 47,
    //
    // Summary:
    //     DNSKEY rfc4034
    DNSKEY = 48,
    //
    // Summary:
    //     NSEC3 rfc5155.
    NSEC3 = 50,
    //
    // Summary:
    //     NSEC3PARAM rfc5155.
    NSEC3PARAM = 51,
    //
    // Summary:
    //     TLSA rfc6698
    TLSA = 52,
    //
    // Summary:
    //     SPF records don't officially have a dedicated RR type, DnsClient.Protocol.ResourceRecordType.TXT
    //     should be used instead. The behavior of TXT and SPF are the same.
    //
    // Remarks:
    //     This library will return a TXT record but will set the header type to SPF if
    //     such a record is returned.
    SPF = 99,
    //
    // Summary:
    //     DNS zone transfer request. This can be used only if DnsClient.DnsQuerySettings.UseTcpOnly
    //     is set to true as AXFR is only supported via TCP.
    //     The DNS Server might only return results for the request if the client connection/IP
    //     is allowed to do so.
    AXFR = 252,
    //
    // Summary:
    //     Generic any query *.
    ANY = 0xFF,
    //
    // Summary:
    //     A Uniform Resource Identifier (URI) resource record.
    URI = 0x100,
    //
    // Summary:
    //     A certification authority authorization.
    CAA = 257,
    //
    // Summary:
    //     A SSH Fingerprint resource record.
    SSHFP = 44
}