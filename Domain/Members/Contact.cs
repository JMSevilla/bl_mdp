using System.Collections.Generic;
using WTW.MdpService.Domain.Common;

namespace WTW.MdpService.Domain.Members;

public class Contact
{
    protected Contact() { }

    public Contact(Address address, Email email, Phone mobilePhone, DataToCopy dataToCopy, string businessGroup, long addressNumber)
    {
        Address = address;
        BusinessGroup = businessGroup;
        AddressNumber = addressNumber;
        Email = email;
        MobilePhone = mobilePhone;
        Data = dataToCopy;
    }

    public string BusinessGroup { get; }
    public long AddressNumber { get; }
    public virtual Address Address { get; }
    public virtual Email Email { get; }
    public virtual Phone MobilePhone { get; }
    public virtual DataToCopy Data { get; }

    public class DataToCopy : ValueObject
    {
        protected DataToCopy() { }

        private DataToCopy(string telephone, string fax, string organizationName, string workMail, string homeMail, string workPhone, string homePhone, string mobilePhone2, string nonUkPostCode)
        {
            Telephone = telephone;
            Fax = fax;
            OrganizationName = organizationName;
            WorkMail = workMail;
            HomeMail = homeMail;
            WorkPhone = workPhone;
            HomePhone = homePhone;
            MobilePhone2 = mobilePhone2;
            NonUkPostCode = nonUkPostCode;
        }

        public string Telephone { get; }
        public string Fax { get; }
        public string OrganizationName { get; }
        public string WorkMail { get; }
        public string HomeMail { get; }
        public string WorkPhone { get; }
        public string HomePhone { get; }
        public string MobilePhone2 { get; }
        public string NonUkPostCode { get; }

        public static DataToCopy Empty()
        {
            return new DataToCopy();
        }

        public DataToCopy Clone()
        {
            return new DataToCopy(Telephone, Fax, OrganizationName, WorkMail, HomeMail, WorkPhone, HomePhone, MobilePhone2, NonUkPostCode);
        }

        protected override IEnumerable<object> Parts()
        {
            yield return Telephone;
            yield return Fax;
            yield return OrganizationName;
            yield return WorkMail;
            yield return HomeMail;
            yield return WorkPhone;
            yield return HomePhone;
            yield return MobilePhone2;
            yield return NonUkPostCode;
        }
    }
}