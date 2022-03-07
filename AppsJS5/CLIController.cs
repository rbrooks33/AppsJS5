using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace brooksoft.appsjs
{
    public class CheckLevel
    {
        public Component Component { get; set; }
        public Component ParentComponent { get; set; }
        public bool DeleteSuccess { get; set; }
        public void Check(Component c, string[] pathArray, int index)
        {
            if (pathArray.Length == index)
            {
                //success
                this.Component = c;
            }
            else
            {
                var children = c.Components.Where(components => components.Name == pathArray[index]);
                if (children.Any())
                {
                    var child = children.First();
                    int nextIndex = index + 1;
                    this.ParentComponent = c;
                    Check(child, pathArray, nextIndex);
                }
                else
                {
                    //failure
                    //this.Success = false;
                }
            }
        }
        public void Delete(Component c, string[] pathArray, int index)
        {
            if (pathArray.Length == index)
            {
                //success
                this.Component = c;
                var foundComponents = this.ParentComponent.Components.Where(c => c.Name == this.Component.Name);
                if (foundComponents.Any())
                    this.ParentComponent.Components.Remove(foundComponents.First());

                this.DeleteSuccess = true;
            }
            else
            {
                var children = c.Components.Where(components => components.Name == pathArray[index]);
                if (children.Any())
                {
                    var child = children.First();
                    int nextIndex = index + 1;
                    this.ParentComponent = c;
                    Delete(child, pathArray, nextIndex);
                }
                else
                {
                    //failure
                    //this.Success = false;
                }
            }
        }
    }

    [Route("api/[controller]")]
    [ApiController]
    public class CLIController : ControllerBase
    {
        [HttpGet]
        [Route("AddComponent")]
        public Result AddComponent(string relativePath, string name)
        {
            var result = new Result();
            try
            {
                var newComponent = new Component(name, "") { Color = "blue", Initialize = true, Load = true, UI = true };

                M("Loading existing components from config...", ref result);

                var components = LoadComponentsConfig();

                M("Got " + components.Components.Count().ToString() + " components from config. Adding component.", ref result);

                string[] paths = relativePath.Split('.');
                
                var headComponent = new Component("", "");
                headComponent.Components = components.Components; //head needs to be of type Component

                var cl = new CheckLevel();
                cl.Check(headComponent, paths, 0);

                if (cl.ParentComponent != null)
                {
                    cl.ParentComponent.Components.Add(newComponent);
                    M("Added component " + newComponent.Name, ref result);

                    SaveComponentsConfig(components);

                    M("Saved to config. Refreshing component on disk.", ref result);

                    result.Success = true;
                }
                else
                    result.FailMessages.Add("Component " + name + " not found.");

            }
            catch (System.Exception ex)
            {
                result.FailMessages.Add("Exception for AddComponent: " + ex.ToString());
            }
            Config.FailMessages.AddRange(result.FailMessages);
            Config.SuccessMessages.AddRange(result.SuccessMessages);
            return result;
        }

        [HttpGet]
        [Route("RemoveComponent")]
        public Result RemoveComponent(string relativePath, string name)
        {
            var result = new Result();
            try
            {
                var components = LoadComponentsConfig();

                string[] paths = relativePath.Split('.');
                //paths.Append(name);

                var headComponent = new Component("", "");
                headComponent.Components = components.Components; //head needs to be of type Component

                var cl = new CheckLevel();
                cl.Delete(headComponent, paths, 0); //deletes the component at end of path

                if (cl.DeleteSuccess)
                {
                    SaveComponentsConfig(components);

                    M("Saved to config. Refreshing component on disk.", ref result);

                    result.Success = true;
                }
                else
                    result.FailMessages.Add("Component " + name + " not found.");

            }
            catch (System.Exception ex)
            {
                result.FailMessages.Add("Exception for AddComponent: " + ex.ToString());
            }
            Config.FailMessages.AddRange(result.FailMessages);
            Config.SuccessMessages.AddRange(result.SuccessMessages);
            return result;
        }

        private void M(string message, ref Result result)
        {
            result.SuccessMessages.Add(message);
        }

        private ComponentReport LoadComponentsConfig()
        {
            var result = new ComponentReport();
            using (StreamReader r = new StreamReader(Config.AppsJSRoot + "/Components/components.js"))
            {
                var json = r.ReadToEnd();
                result = JsonSerializer.Deserialize<ComponentReport>(json);
            }
            return result;
        }
        public static void SaveComponentsConfig(ComponentReport components)
        {
            using (StreamWriter w = new StreamWriter(Config.AppsJSRoot + "/Components/components.js"))
            {
                w.Write(JsonSerializer.Serialize(components));
            }
        }

        [HttpPost]
        [Route("DeleteComponent")]
        public Result DeleteComponent(string appsRoot, Component component)
        {
            var result = new Result();
            try
            {
                    M("Config is valid, loading components from config...", ref result);
                    var components = LoadComponentsConfig();

                    M("Got components (" + components.Components.Count().ToString() + "). Removing component from config:" + component.Name, ref result);
                    components.Components.RemoveAll(c => c.Name == component.Name);

                    M("Removed components. New component count is " + components.Components.Count().ToString() + ". Saving components...", ref result);
                    SaveComponentsConfig(components);

                    M("Component removed from config. Removing from disk...", ref result);
                    string componentFolderPath = appsRoot + "\\Components\\" + component.Name;

                    M("Got full component path: " + componentFolderPath + ". Checking if folder exists...", ref result);
                    bool folderExists = Directory.Exists(componentFolderPath);
                    if (folderExists)
                    {
                        M("Folder exists. Deleting...", ref result);
                        Directory.Delete(componentFolderPath, true);

                        M("Component folder deleted.", ref result);
                    }
                    else
                        M("Component folder was not on disk.", ref result);

                    result.Success = true;
            }
            catch (System.Exception ex)
            {
                M("Exception in DeleteComponent: " + ex.Message + ". Stack: " + ex.StackTrace, ref result);
                //result.Data = ex;
            }
            return result;
        }
        //[HttpGet]
        //[Route("RefreshAllComponents")]
        //public Result RefreshAllComponents()
        //{
        //    var result = new Result();
        //    try
        //    {
        //        if (Config.IsValid)
        //        {
        //            //result = RefreshComponents();
        //            result.Success = true;
        //        }
        //        //else
        //        //    result.FailMessages.Add("Config was not valid for Refresh All Components.");
        //    }
        //    catch (Exception ex)
        //    {
        //        result.Data = ex;
        //    }
        //    return result;
        //}

        /// <summary>
        /// Makes sure all config components are created on disk
        /// </summary>
        /// <returns></returns>
        //private void RefreshComponents(ref Result result)
        //{
        //    try
        //    {
        //        M("Getting current config...", ref result);
        //        var config = Config.CurrentConfig;

        //        M("Loading components from config...", ref result);
        //        var components = Config.LoadComponentsConfig().Components;

        //        M("Loading base component folder...", ref result);
        //        var componentFolder = new DirectoryInfo(config.BaseComponentsFolder);

        //        M("Got base component folder " + componentFolder.FullName + ". Getting templates folder...", ref result);
        //        //var templateFolder = new DirectoryInfo(config.BaseTemplatesFolder);

        //       // M("Got base template folder " + templateFolder.FullName + ". Creating components as needed...", ref result);
        //        //CreateComponents(components, componentFolder, templateFolder, ref result);

        //        //result.Success = true;
        //    }
        //    catch (System.Exception ex)
        //    {
        //        M("Exception creating component: " + ex.Message + ". Stack: " + ex.StackTrace, ref result);
        //        //result.Data = ex;
        //    }
        //}

        //private void CreateComponents(List<Component> componentList, DirectoryInfo componentFolder, DirectoryInfo templateFolder, ref Result result)
        //{
        //    M("Getting current config...", ref result);
        //    var config = Config.CurrentConfig;

        //    M("Going through component list of " + componentList.Count().ToString(), ref result);
        //    foreach (Component c in componentList)
        //    {
        //        M("Checking if component folder alread exists...", ref result);
        //        if (Directory.Exists(c.ComponentFolder))
        //        {
        //            M("Exists. ", ref result);
        //            componentFolder = new DirectoryInfo(c.ComponentFolder);
        //        }
        //        M("Checking if main template folder exists...", ref result);
        //        if (Directory.Exists(c.TemplateFolder))
        //        {
        //            M("Template folder exists.", ref result);
        //            templateFolder = new DirectoryInfo(c.TemplateFolder);
        //        }
        //        M("Creating component...", ref result);
        //        CreateComponent(componentFolder.FullName + "\\" + c.Name, templateFolder.FullName, c.Name, ref result);

        //        M("Checking for sub-components...", ref result);
        //        if (c.Components.Count > 0)
        //        {
        //            M("Sub-component found: " + c.Components.Count.ToString() + ". Getting sub component folder...", ref result);
        //            string subComponentFolderPath = componentFolder.FullName + "\\" + c.Name + "\\Components";

        //            M("Got folder: " + subComponentFolderPath + ". Checking if already exists...", ref result);
        //            if (!Directory.Exists(subComponentFolderPath))
        //            {
        //                M("Doesn't exist, creating...", ref result);
        //                Directory.CreateDirectory(subComponentFolderPath);
        //            }
        //            var subComponentFolder = new DirectoryInfo(subComponentFolderPath);

        //            M("Calling myself (CreateComponents) for any deeper sub-components...", ref result);
        //            CreateComponents(c.Components, subComponentFolder, templateFolder, ref result);
        //        }
        //    }
        //}
        //private void CreateComponent(string appsRoot, string componentName, ref Result result)
        //{
        //    M("Checking if component path exists...", ref result);
        //    if (!Directory.Exists(appsRoot + "\\Components\\"componentPath))
        //    {
        //        M("Didn't exist, creating...", ref result);
        //        Directory.CreateDirectory(componentPath);

        //        M("Creating all template files...", ref result);
        //        CreateComponentPage(templatesPath + "\\empty.js", componentPath + "\\" + componentName + ".js", componentName, ref result);
        //        CreateComponentPage(templatesPath + "\\empty.html", componentPath + "\\" + componentName + ".html", componentName,ref result);
        //        CreateComponentPage(templatesPath + "\\empty.css", componentPath + "\\" + componentName + ".css", componentName, ref result);
        //    }
        //    else
        //        M("Directory " + componentPath + " already exists. No need to create.", ref result);
        //}
        //private void CreateComponentPage(string templatePath, string componentPagePath, string componentName, ref Result result)
        //{
        //    M("Checking if file exists: " + componentPagePath, ref result);
        //    if (!System.IO.File.Exists(componentPagePath))
        //    {
        //        M("Exists. Correcting relative file paths...", ref result);
        //        string relativePath = componentPagePath.Replace(Config.CurrentConfig.BaseComponentsFolder, "");
        //        relativePath = relativePath.Replace("\\", "/"); //switch to html delimiters
        //        relativePath = relativePath.Replace(componentName + ".js", ""); //remove trailing file
        //        if(relativePath.Length > 5)
        //            relativePath = relativePath.Substring(1, relativePath.Length - 2); //remove before and after slashes

        //        M("Reading contents...", ref result);
        //        string htmlText = System.IO.File.ReadAllText(templatePath);
        //        M("Replacing component name...", ref result);
        //        htmlText = htmlText.Replace("MyTemplate", componentName);
        //        M("Replacing file paths...", ref result);
        //        htmlText = htmlText.Replace("MyRelativePath", relativePath);
        //        M("Writing file...", ref result);
        //        System.IO.File.WriteAllText(componentPagePath, htmlText);
        //    }
        //    else
        //        M("Page " + componentPagePath + " already exists, no need to create.", ref result);
        //}
    }
}
