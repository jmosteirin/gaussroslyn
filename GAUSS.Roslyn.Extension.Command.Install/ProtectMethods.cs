//------------------------------------------------------------------------------
// <copyright file="ProtectMethods.cs" company="F4E">
//     Copyright (c) GAUSS.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel.Design;
using System.Globalization;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System.Linq;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Options;
using Microsoft.CodeAnalysis.CSharp.Formatting;
using Microsoft.CodeAnalysis.Formatting;
using System.IO;

namespace GAUSS.Roslyn.Extension.Command.Install
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class ProtectMethods
    {
        Microsoft.VisualStudio.LanguageServices.VisualStudioWorkspace workspace = null;
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("51a6ff96-33ed-4b15-9d13-728bfeac1dee");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly Package package;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProtectMethods"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private ProtectMethods(Package package)
        {
            if (package == null)
            {
                throw new ArgumentNullException("package");
            }

            this.package = package;

            IComponentModel componentModel = (IComponentModel)this.ServiceProvider.GetService(typeof(SComponentModel));
            workspace = componentModel.GetService<Microsoft.VisualStudio.LanguageServices.VisualStudioWorkspace>();

            OleMenuCommandService commandService = this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                var menuCommandID = new CommandID(CommandSet, CommandId);
                var menuItem = new MenuCommand(this.MenuItemCallback, menuCommandID);
                commandService.AddCommand(menuItem);
            }
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static ProtectMethods Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private IServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static void Initialize(Package package)
        {
            Instance = new ProtectMethods(package);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void MenuItemCallback(object sender, EventArgs e)
        {
            var dte = (DTE2)this.ServiceProvider.GetService(typeof(DTE));
            var activeDocument = dte.ActiveDocument;

            if(activeDocument != null)
            {

                var documentid = workspace.CurrentSolution.GetDocumentIdsWithFilePath(activeDocument.FullName).FirstOrDefault();

                if (documentid != null)
                {
                    var document = workspace.CurrentSolution.GetDocument(documentid);

                    var syntaxRoot = document.GetSyntaxRootAsync().Result;
                    var Mscorlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
                    var compilation = CSharpCompilation.Create("MyCompilation",
                        syntaxTrees: new[] { syntaxRoot.SyntaxTree }, references: new[] { Mscorlib });
                    var model = compilation.GetSemanticModel(syntaxRoot.SyntaxTree);

                    var newRoot = (new GAUSS.Roslyn.Common.ProtectMethodsRewriter(model)).Visit(syntaxRoot);

                    OptionSet options = workspace.Options;
                    options = options.WithChangedOption(CSharpFormattingOptions.NewLinesForBracesInMethods, false);
                    options = options.WithChangedOption(CSharpFormattingOptions.NewLinesForBracesInTypes, false);
                    SyntaxNode formattedNode = Formatter.Format(newRoot, workspace, options);

                    var objTextDoc = (EnvDTE.TextDocument)activeDocument.Object("TextDocument");
                    objTextDoc.Selection.SelectAll();
                    objTextDoc.Selection.ReplaceText(objTextDoc.Selection.Text, formattedNode.ToString());
                }

            }
        }

    }
}
