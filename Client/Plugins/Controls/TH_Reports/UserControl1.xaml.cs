using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
//using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using MigraDoc.DocumentObjectModel;

namespace TH_Reports
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class UserControl1 : UserControl
    {
        public UserControl1()
        {
            InitializeComponent();







        }



        static Document CreateDocument()
        {

            Document Result = new Document();

            Section section = Result.AddSection();

            Paragraph paragraph = section.AddParagraph();

            paragraph.Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Red;

            paragraph.AddFormattedText("HELLO WORLD!", TextFormat.Bold);

            return Result;

        }
    }
}
