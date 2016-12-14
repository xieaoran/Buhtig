using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Buhtig.Entities.User;

namespace Buhtig.Converters
{
    public class UserCommitsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;
            if (value.GetType() == typeof(Team))
            {
                var team = (Team) value;
                return team.Repo.Commits;
            }
            if (value.GetType() == typeof(Student))
            {
                var student = (Student) value;
                return student.Commits;
            }
            throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
