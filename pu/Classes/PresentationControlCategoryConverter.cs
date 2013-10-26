using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Windows.Data;
using Prism.General;
using Prism.Properties;

namespace Prism.Classes
{
    [ValueConversion(typeof(PresentationControlCategory), typeof(string))]
    public class PresentationControlCategoryConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)        
        {
            if (value is PresentationControlCategory)
            {
                string result = "";

                switch ((PresentationControlCategory)value)
                {
                    case PresentationControlCategory.Category0:
                        result = Resources.ENUM_PRESENTATIONCONTROLCATEGORY_0;
                        break;
                    case PresentationControlCategory.Category1:
                        result = Resources.ENUM_PRESENTATIONCONTROLCATEGORY_1;
                        break;
                    case PresentationControlCategory.Category2:
                        result = Resources.ENUM_PRESENTATIONCONTROLCATEGORY_2;
                        break;
                    case PresentationControlCategory.Category3:
                        result = Resources.ENUM_PRESENTATIONCONTROLCATEGORY_3;
                        break;
                }

                if (parameter is string)
                {
                    if (((string)parameter).Equals("lowercase"))
                    {
                        return result.ToLower();
                    }
                    else if (((string)parameter).Equals("uppercase"))
                    {
                        return result.ToUpper();
                    }
                }

                return result;
            }
            return "";        
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new Exception("Can't convert back to PresentationControlCategory");
        }
    }
}
