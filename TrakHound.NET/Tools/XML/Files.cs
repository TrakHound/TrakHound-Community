//  Copyright 2016 Feenux LLC
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.

using System;
using System.IO;
using System.Xml;

using TrakHound.Logging;

namespace TrakHound.Tools.XML
{
    public static class Files
    {

        public static bool WriteDocument(XmlDocument doc, string path, bool overwrite = false)
        {
            var settings = new XmlWriterSettings();
            settings.Indent = true;

            int attempt = 0;
            int maxAttempts = 3;
            bool success = false;

            while (!success && attempt < maxAttempts)
            {
                try
                {
                    FileMode mode = FileMode.OpenOrCreate;
                    if (overwrite) mode = FileMode.Create;

                    using (var fs = new FileStream(path, mode, FileAccess.Write, FileShare.ReadWrite))
                    {
                        using (var writer = XmlWriter.Create(fs, settings))
                        {
                            doc.Save(writer);
                            success = true;
                        }
                    }
                }
                catch (XmlException ex) { Logger.Log("XmlException :: " + ex.Message); }
                catch (Exception ex) { Logger.Log("Exception :: " + ex.Message); }

                if (!success) System.Threading.Thread.Sleep(50);

                attempt++;
            }

            return success;
        }

        public static XmlDocument ReadDocument(string path, XmlReaderSettings settings = null)
        {
            if (File.Exists(path))
            {
                int attempt = 0;
                int maxAttempts = 3;
                bool success = false;

                while (!success && attempt < maxAttempts)
                {
                    try
                    {
                        using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                            using (var reader = XmlReader.Create(fs, settings))
                            {
                                var xml = new XmlDocument();
                                xml.Load(reader);
                                
                                success = true;
                                return xml;
                            }
                        }
                    }
                    catch (XmlException ex) { Logger.Log("XmlException :: " + ex.Message); }
                    catch (Exception ex) { Logger.Log("Exception :: " + ex.Message); }

                    if (!success) System.Threading.Thread.Sleep(50);

                    attempt++;
                }
            }

            return null;
        }

    }
}
