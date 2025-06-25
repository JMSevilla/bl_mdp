using System;
using System.Collections.Generic;

namespace WTW.MdpService.Domain.Mdp;

public partial class DomainList
{
    public string BusinessGroup { get; set; }
    public string Domain { get; set; }
    public int SequenceNumber { get; set; }
    public string ListOfValidValues { get; set; }
    public string TitleOfValidValues { get; set; }
}
