using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RazorTemplates.Core;
using SqlScriptGenerator.Models;

namespace SqlScriptGenerator
{
    class Generator
    {
        public string ApplyModelToTemplate(string templatePath, TemplateModel model)
        {
            if(!File.Exists(templatePath)) {
                throw new FileNotFoundException($"Could not load template {templatePath}, it does not exist");
            }

            var templateSource = File.ReadAllText(templatePath);
            var template = Template.Compile<TemplateModel>(templateSource);
            var result = template.Render(model);

            return result;
        }
    }
}
