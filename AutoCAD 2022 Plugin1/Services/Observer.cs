using System.Collections.Generic;

namespace AutoCAD_2022_Plugin1.Services
{
    public interface IObservable
    {
        void AddObserver(IObserver obs);
        void NotifyObservers();
        void RemoveObserver(IObserver obs);
    }

    public interface IObserver
    {
        void Update();
    }

    public class CurrentLayoutObservable : IObservable
    {
        public string CurrentLayout { get; private set; }
        public string CurrentViewport { get; private set; }

        private List<IObserver> Observers;

        public CurrentLayoutObservable(string CurrentLayout)
        {
            Observers = new List<IObserver>();
            this.CurrentLayout = CurrentLayout;
        }

        public void AddObserver(IObserver obs)
        {
            Observers.Add(obs);
        }

        public void NotifyObservers()
        {
            foreach (IObserver obs in Observers)
            {
                obs.Update();
            }
        }

        public void RemoveObserver(IObserver obs)
        {
            Observers.Remove(obs);
        }

        public void UpdateCurrentLayout(string Current)
        {
            this.CurrentLayout = Current;
            NotifyObservers();
        }

        public void UpdateCurrentViewport(string Current)
        {
            this.CurrentViewport = Current;
            NotifyObservers();
        }
    }
}
