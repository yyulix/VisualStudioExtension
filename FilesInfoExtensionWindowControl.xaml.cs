using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using System;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using System.Text.RegularExpressions;


namespace FilesInfoExtension
{
    public partial class FilesInfoExtensionWindowControl : UserControl
    {
        private List<FunctionStats> stats;

        string commentRegex = @"(/\*((.*?)|(((.*?)\n)+(.*?)))\*/|//((\n)|((.*?)[^\\]\n)))";
        string quoteRegex = @"(""(("")|(\n)|((.*?)(([^\\]\n)|([^\\]""))))|'((')|(\n)|((.*?)(([^\\]\n)|([^\\]')))))";
        string emptyLinesRegex = @"\n\s*\n";

        public class FunctionStats
        {
            public string functionName { get; set; }
            public int amountOfLines { get; set; }
            public int amountOfPayloadLines { get; set; }
            public int amountOfKeywords { get; set; }
        }

        public FilesInfoExtensionWindowControl()
        {
            this.InitializeComponent();
            stats = new List<FunctionStats>();
            StatisticsListView.ItemsSource = stats;
        }


        string NoComments(string code) =>
            Regex.Replace(code, commentRegex + "|" + quoteRegex, m =>
                {
                    if (m.Value.StartsWith("//")) return Environment.NewLine;
                    if (m.Value.StartsWith("/*") && m.Value.Split('\n').Length == 1) return "";
                    if (m.Value.StartsWith("/*")) return Environment.NewLine;

                    return m.Value;
                }, RegexOptions.Singleline);

        string NoQuotes(string code) =>
            Regex.Replace(code, quoteRegex, m =>
                {
                    if (m.Value.StartsWith("\"")) return Environment.NewLine;
                    if (m.Value.StartsWith("\'")) return Environment.NewLine;
                    return m.Value;
                }, RegexOptions.Singleline);

        string keywordsRegex = @"(\s|(\d+)|\b)" +
                                   @"(alignas|alignof|and|and_eq|asm|auto|bitand|bitor|bool|break|case|catch|char|char16_t|char32_t|" +
                                   @"class|compl|const|constexpr|const_cast|continue|decltype|default|delete|do|double|dynamic_cast|" +
                                   @"else|enum|explicit|export|extern|false|float|for|friend|goto|if|inline|int|long|mutable|namespace|" +
                                   @"new|noexcept|not|not_eq|nullptr|operator|or|or_eq|private|protected|public|register|reinterpret_cast|" +
                                   @"return|short|signed|sizeof|static|static_assert|static_cast|struct|switch|template|this|thread_local|" +
                                   @"throw|true|try|typedef|typeid|typename|union|unsigned|using|virtual|void|volatile|wchar_t|while|xor|xor_eq)" +
                                   @"(\s|\b)";


        int getAmountOfLines(string code)
        {
            return code.Split('\n').Length;
        }

        int getAmountOfKeywords(string code)
        {
            return Regex.Matches(code, keywordsRegex).Count;
        }
        private void AnalyseFunction(CodeElement functionElement)
        {
            var functionCode = functionElement as CodeFunction;

            var functionStart = functionCode.GetStartPoint(vsCMPart.vsCMPartHeader);
            var functionEnd = functionCode.GetEndPoint(vsCMPart.vsCMPartBodyWithDelimiter);

            var code = functionStart.CreateEditPoint().GetText(functionEnd);

            if (code.IndexOf('{') >= 0)
            {
                var codeWithoutEmptyLinesAndComments = (new Regex(emptyLinesRegex)).Replace(NoComments(code), "\n");
                var codeWithoutEmptyLinesAndCommentsAndQuotes = (new Regex(emptyLinesRegex)).Replace(NoQuotes(codeWithoutEmptyLinesAndComments), "\n");

                stats.Add(new FunctionStats() {
                    functionName = functionElement.FullName,
                    amountOfLines = getAmountOfLines(code),
                    amountOfPayloadLines = getAmountOfLines(codeWithoutEmptyLinesAndComments),
                    amountOfKeywords = getAmountOfKeywords(codeWithoutEmptyLinesAndCommentsAndQuotes)

            }); ;
            }
        }

        private void AnalyseClass(CodeElement codeElement)
        {
            foreach (CodeElement element in codeElement.Children)
            {
                if (element.Kind == vsCMElement.vsCMElementFunction)
                {
                    AnalyseFunction(element);
                }

                else if (element.Kind == vsCMElement.vsCMElementClass)
                {
                    AnalyseClass(element);
                }
            }

        }

        private void AnalyseFile(FileCodeModel2 file)
        {
            Dispatcher.VerifyAccess();

            foreach (CodeElement element in file.CodeElements)
            {
                if (element.Kind == vsCMElement.vsCMElementFunction)
                {
                    AnalyseFunction(element);
                }
                else if (element.Kind == vsCMElement.vsCMElementClass)
                {
                    AnalyseClass(element);
                }
            }
        }

        private void MenuItemCallback()
        {
            Dispatcher.VerifyAccess();
            stats.Clear();

            try
            {
                var file = (FileCodeModel2)(((DTE2)ServiceProvider.GlobalProvider.GetService(typeof(DTE))).ActiveDocument.ProjectItem).FileCodeModel;
                AnalyseFile(file);

            }
            catch (Exception ex)
            {

            }
        }

        private void RefreshButton_OnClick(object sender, RoutedEventArgs e)
        {
            MenuItemCallback();
            StatisticsListView.Items.Refresh();
        }

        private void StatisticsListView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var columnWidth = (sender as ListView).ActualWidth / 4;

            foreach (var column in ((sender as ListView).View as GridView).Columns)
            {
                column.Width = columnWidth;
            }
        }

        private void StatisticsListView_Loaded(object sender, RoutedEventArgs e)
        {
            var columnWidth = (sender as ListView).ActualWidth / 4;

            foreach (var column in ((sender as ListView).View as GridView).Columns)
            {
                column.Width = columnWidth;
            }
        }
    }
}