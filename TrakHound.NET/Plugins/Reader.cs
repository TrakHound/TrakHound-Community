// Copyright (c) 2017 TrakHound Inc., All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE', which is part of this source code package.

using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Reflection;

namespace TrakHound.Plugins
{
    public interface ReaderContainer
    {
        IEnumerable<Lazy<object>> Plugins { get; set; }
    }

    public static class Reader
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public static List<T> FindPlugins<T>(string path, ReaderContainer readerContainer, string extension = null)
        {
            var result = new List<T>();

            if (path != null && readerContainer != null)
            {
                CompositionContainer container = null;

                // path is to an individual file
                if (System.IO.File.Exists(path))
                {
                    string ext = System.IO.Path.GetExtension(path);

                    // Check that the file extension is correct
                    if (ext != null && extension != null)
                    {
                        if (ext.ToLower() == extension)
                        {
                            var assembly = GetAssemblyFromPath(path);
                            if (assembly != null)
                            {
                                var assemblyCatalog = new AssemblyCatalog(assembly);
                                container = new CompositionContainer(assemblyCatalog);
                            }
                        }
                    }
                }
                // path is to a directory
                else if (System.IO.Directory.Exists(path))
                {
                    try
                    {
                        DirectoryCatalog directoryCatalog;
                        if (extension != null) directoryCatalog = new DirectoryCatalog(path, "*" + extension);
                        else directoryCatalog = new DirectoryCatalog(path);

                        container = new CompositionContainer(directoryCatalog);
                    }
                    catch (UnauthorizedAccessException ex) { logger.Error(ex); }
                    catch (Exception ex) { logger.Error(ex); }
                }

                if (container != null)
                {
                    // Try Loading the Imports (Plugins)
                    try
                    {
                        container.SatisfyImportsOnce(readerContainer);
                    }
                    catch (ReflectionTypeLoadException ex)
                    {
                        logger.Error(ex);
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex);
                    }

                    if (readerContainer.Plugins != null)
                    {
                        foreach (var lPlugin in readerContainer.Plugins)
                        {
                            try
                            {
                                var plugin = (T)lPlugin.Value;
                                result.Add(plugin);
                            }
                            catch (Exception ex)
                            {
                                logger.Error(ex);
                            }
                        }
                    }
                }
            }

            return result;
        }

        public static List<T> FindPlugins<T>(Assembly assembly, ReaderContainer readerContainer)
        {
            var result = new List<T>();

            if (readerContainer != null)
            {
                CompositionContainer container = null;

                if (assembly != null)
                {
                    var assemblyCatalog = new AssemblyCatalog(assembly);
                    container = new CompositionContainer(assemblyCatalog);
                }

                if (container != null)
                {
                    // Try Loading the Imports (Plugins)
                    try
                    {
                        container.SatisfyImportsOnce(readerContainer);
                    }
                    catch (ReflectionTypeLoadException ex)
                    {
                        logger.Error(ex);
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex);
                    }

                    if (readerContainer.Plugins != null)
                    {
                        foreach (var lPlugin in readerContainer.Plugins)
                        {
                            try
                            {
                                var plugin = (T)lPlugin.Value;
                                result.Add(plugin);
                            }
                            catch (Exception ex)
                            {
                                logger.Error(ex);
                            }
                        }
                    }
                }
            }

            return result;
        }

        public static List<Lazy<object>> FindLazyPlugins<T>(string path, ReaderContainer readerContainer, string extension = null)
        {
            var result = new List<Lazy<object>>();

            if (path != null && readerContainer != null)
            {
                CompositionContainer container = null;

                // path is to an individual file
                if (System.IO.File.Exists(path))
                {
                    string ext = System.IO.Path.GetExtension(path);

                    // Check that the file extension is correct
                    if (ext != null && extension != null)
                    {
                        if (ext.ToLower() == extension)
                        {
                            var assembly = GetAssemblyFromPath(path);
                            if (assembly != null)
                            {
                                var assemblyCatalog = new AssemblyCatalog(assembly);
                                container = new CompositionContainer(assemblyCatalog);
                            }
                        }
                    }
                }
                // path is to a directory
                else if (System.IO.Directory.Exists(path))
                {
                    DirectoryCatalog directoryCatalog;
                    if (extension != null) directoryCatalog = new DirectoryCatalog(path, "*" + extension);
                    else directoryCatalog = new DirectoryCatalog(path);

                    container = new CompositionContainer(directoryCatalog);
                }

                if (container != null)
                {
                    // Try Loading the Imports (Plugins)
                    try
                    {
                        container.SatisfyImportsOnce(readerContainer);
                    }
                    catch (ReflectionTypeLoadException ex)
                    {
                        logger.Error(ex);
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex);
                    }

                    if (readerContainer.Plugins != null)
                    {
                        foreach (var lPlugin in readerContainer.Plugins)
                        {
                            result.Add(lPlugin);
                        }
                    }
                }
            }

            return result;
        }

        static Assembly GetAssemblyFromPath(string path)
        {
            Assembly result = null;

            try
            {
                result = Assembly.LoadFile(path);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }

            return result;
        }

    }
}
