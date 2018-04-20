using DataAccess.SqlServer;
using DLMS.SupportClasses;
using Helpers;
using SupportClasses;
using System.Xml.Serialization;

namespace ViewModels
{
    public abstract class BaseVM
    {
        private IDataContext _dc;
        [XmlIgnore]
        public IDataContext DC
        {
            get
            {
                if (_dc == null)
                {
                        _dc = new DataContext();
                }
                return _dc;
            }
            set
            {
                _dc = value;
            }
        }
        public OCFUser FFUser { get; set; }
        [XmlIgnore]
        public FFSession Session { get; set; }
        public SupportedLanguage CurrentLanguage
        {
            get
            {
                return UtilsTool.GetCurrentLanguage();
            }
        }
    }
}