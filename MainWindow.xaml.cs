using IDChecker.Models;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using System.Xml;

namespace IDChecker
{

    public partial class MainWindow : Window
    {
        List<XmlNode> elements = new List<XmlNode>();
        List<string> controlsToTrack = new List<string>();
        List<MissingID> _missingIDs = new List<MissingID>();
        string filePath;
        bool mustContainName = false;
        bool mustContainXName = true;


        public MainWindow()
        {
            InitializeComponent();
            controlsToTrack.Add("Label");
            controlsToTrack.Add("Button");
            controlsToFindLB.ItemsSource = controlsToTrack;

        }

        private void GetMissingIds()
        {
            mustContainName = (mcncb.IsChecked.HasValue && mcncb.IsChecked.Value == true);
            mustContainXName = (mcxncb.IsChecked.HasValue && mcxncb.IsChecked.Value == true);

            missingItems.ItemsSource = null;
            GetXaml();

            for (int i = 0; i < elements.Count; i++)
            {
                _missingIDs.Add(new MissingID(elements[i].Name, GetLineNumber(elements[i].OuterXml), elements[i].OuterXml));
            }
            missingItems.ItemsSource = _missingIDs;

            itemCount.Content = "Count: " + _missingIDs.Count;

            if (_missingIDs.Count == 0)
            {
                MessageBox.Show("All Tracked controls have an ID!");
            }
        }


        private void GetXaml()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(filePath);

            AddElements(doc.ChildNodes);
        }

        private void AddElements(XmlNodeList nodes)
        {

            foreach (XmlNode node in nodes)
            {
                /*
                if(mustContainXName && !node.OuterXml.Contains("x:Name") || mustContainName && !node.OuterXml.Contains("Name"))
                {
                    elements.Add(node);
                }
                */

                if(MatchesControlName(node.Name))
                { 

                bool foundName = false;
                bool foundxName = false;
                for (int i = 0; i < node.Attributes.Count; i++)
                {
                    if (node.Attributes[i].Name.Contains("Name"))
                    {
                        foundName = true;
                    }

                    if (node.Attributes[i].Name.Contains("x:Name"))
                    {
                        foundxName = true;
                    }
                }

                if (mustContainXName && !foundxName || mustContainName && !foundName)
                {
                    elements.Add(node);
                }
                }

                GetChildElements(node);

            }
        }

        private void GetChildElements(XmlNode node)
        {
            foreach (XmlNode childnode in node.ChildNodes)
            {
                if (MatchesControlName(childnode.Name))
                {

                    /*
                    if (mustContainXName && !childnode.OuterXml.Contains("x:Name") || mustContainName && !childnode.OuterXml.Contains("Name"))
                    {
                        elements.Add(childnode);
                    }
                    */
                    bool foundName = false;
                    bool foundxName = false;
                    for (int i = 0; i < childnode.Attributes.Count; i++)
                    {
                        if(childnode.Attributes[i].Name.Contains("Name"))
                        {
                            foundName = true;
                        }

                        if (childnode.Attributes[i].Name.Contains("x:Name"))
                        {
                            foundxName = true;
                        }
                    }

                    if (mustContainXName && !foundxName || mustContainName && !foundName)
                    {
                        elements.Add(childnode);
                    }
                }
                GetChildElements(childnode);
            }
        }


        private bool MatchesControlName(string name)
        {
            foreach (string s in controlsToTrack)
            {
                if (s.Equals(name))
                {
                    return true;
                }
            }

            return false;
        }

        private int GetLineNumber(string s)
        {
            string[] lines = System.IO.File.ReadAllLines(filePath);

            for (int i = 0; i < lines.Length; i++)
            {

                string trimmed = lines[i].Trim();
                string trimmedmatch = s.Trim();
                string halfmatch = trimmedmatch.Substring(0, (trimmedmatch.Length / 2));

                string nowhitespacetemplate = Regex.Replace(RemoveSpecialCharacters(trimmed), @"\s+", "");
                string nowhitespacematch = Regex.Replace(RemoveSpecialCharacters(halfmatch), @"\s+", "");

                if (nowhitespacetemplate.Contains(nowhitespacematch))
                {
                    return i;
                }

            }
            return -1;
        }

        private string RemoveSpecialCharacters(string str)
        {
            return Regex.Replace(str, @"[^\w\d\s]", "");
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Xaml Files (*.xaml)|*.xaml";

            if (openFileDialog.ShowDialog() == true)
            {
                reEvaluate.IsEnabled = true;
                _missingIDs.Clear();
                elements.Clear();
                filePath = openFileDialog.FileName;
                GetMissingIds();
            }
        }

        private void ClearComponents(object sender, RoutedEventArgs e)
        {
            controlsToFindLB.ItemsSource = null;
            controlsToTrack.Clear();
            controlsToFindLB.ItemsSource = controlsToTrack;

        }

        private void AddComponent(object sender, RoutedEventArgs e)
        {
            controlsToFindLB.ItemsSource = null;
            controlsToTrack.Add(addControlTB.Text);
            controlsToFindLB.ItemsSource = controlsToTrack;
            addControlTB.Text = "";
        }

        private void ReEvaluate(object sender, RoutedEventArgs e)
        {
            _missingIDs.Clear();
            elements.Clear();
            GetMissingIds();
        }
    }
}
