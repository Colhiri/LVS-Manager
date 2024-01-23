using Autodesk.AutoCAD.Runtime;

using System;

using AcCoreAp = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace AutoCAD_2022_Plugin1.Services
{
    public class Initialization : IExtensionApplication
    {
        public void Initialize()
        {
            AcCoreAp.Idle += OnIdle;
        }

        private void OnIdle(object sender, EventArgs e)
        {
            var doc = AcCoreAp.DocumentManager.MdiActiveDocument;
            if (doc != null)
            {
                AcCoreAp.Idle -= OnIdle;
                doc.Editor.WriteMessage("\nAutoCAD 2022 Plugin1 loaded.\n");
            }
        }

        public void Terminate()
        { }
    }
}
