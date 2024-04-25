using AutoCAD_2022_Plugin1.Models;
using AutoCAD_2022_Plugin1.Services;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.GraphicsSystem;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Linq;

namespace AutoCAD_2022_Plugin1.ViewModels.ManageVM
{
    public class ManageVIewportVM : MainVM, IMyTabContentViewModel, IObserver
    {
        private CurrentLayoutObservable obs;
        private ManageViewportModel Model;

        public ManageVIewportVM(ManageViewportModel Model, CurrentLayoutObservable obs) 
        {
            Model = new ManageViewportModel();
            this.obs = obs;
            obs.AddObserver(this);
        }

        private event Func<string> CheckFieldNameEvent;
        private event Func<bool> CheckTabEnabledEvent;
        private string NameField => CheckFieldNameEvent();

        public bool CheckTabEnabled
        {
            get
            {
                return CheckTabEnabledEvent();
            }
        }

        /// Проверка корректности имени
        private bool _ApplyButtonEnabled;
        public bool ApplyButtonEnabled
        {
            get
            {
                _ApplyButtonEnabled = Model.IsValidScale(AnnotationScaleObjectsVP);
                if (_ApplyButtonEnabled == false)
                {
                    Application.ShowAlertDialog("Введен неправильный масштаб! Выберите другой или исправьте существующий!");
                }
                return _ApplyButtonEnabled;
            }
        }

        /// Проверка редактирования некоторых частей View
        public bool EnabledFormsParamaters
        {
            get
            {
                if (ViewportName == null) return false;
                return Model.CheckToDelete();
            }
        }
        public bool InvertEnabledForms
        {
            get
            {
                if (ViewportName == null) return false;
                return !EnabledFormsParamaters;
            }
        }

        /// Взаимодействие видовых экранов
        private ObservableCollection<string> _Viewports;
        public ObservableCollection<string> Viewports
        {
            get
            {
                var viewportsIDs = 
                _Viewports = new ObservableCollection<string>(viewportsIDs);
                return _Viewports;
            }
        }
        private string _ViewportName;
        public string ViewportName
        {
            get
            {
                return _ViewportName;
            }
            set
            {
                _ViewportName = value;
                OnPropertyChanged(nameof(ViewportName));
                OnPropertyChanged(nameof(AnnotationScaleObjectsVP));
                OnPropertyChanged(nameof(EnabledFormsParamaters));
                OnPropertyChanged(nameof(InvertEnabledForms));
            }
        }

        /// Взаимодействие с мастабами видовых экранов
        private ObservableCollection<string> _Scales;
        public ObservableCollection<string> Scales
        {
            get
            {
                if (ViewportName == null)
                {
                    return null;
                }
                _Scales = new ObservableCollection<string>(CadUtilityLib.GetAllAnnotationScales());
                return _Scales;
            }
        }
        private string _AnnotationScaleObjectsVP;
        public string AnnotationScaleObjectsVP
        {
            get
            {
                return _AnnotationScaleObjectsVP;
            }
            set
            {
                _AnnotationScaleObjectsVP = value.Trim();
            }
        }

        /// <summary>
        /// Добавление имени макета в список на удаление
        /// </summary>
        private void AddDelete()
        {
            if (ViewportName == null) return;
            Model.SetDelete();
            OnPropertyChanged(nameof(EnabledFormsParamaters));
            OnPropertyChanged(nameof(InvertEnabledForms));
        }
        private RelayCommand _DeleteCommand;
        public RelayCommand DeleteCommand
        {
            get
            {
                if (_DeleteCommand == null)
                {
                    _DeleteCommand = new RelayCommand(o => AddDelete(), null);
                }
                return _DeleteCommand;
            }
        }

        /// <summary>
        /// Убрать макета или видовой экран из списка на удаление
        /// </summary>
        private void RemoveDelete() 
        {
            if (ViewportName == null) return;
            Model.RemoveDelete();
            OnPropertyChanged(nameof(EnabledFormsParamaters));
            OnPropertyChanged(nameof(InvertEnabledForms));
        }
        private RelayCommand _CancelDeleteCommand;
        public RelayCommand CancelDeleteCommand
        {
            get
            {
                if (_CancelDeleteCommand == null)
                {
                    _CancelDeleteCommand = new RelayCommand(o => RemoveDelete(), null);
                }
                return _CancelDeleteCommand;
            }
        }
        
        /// <summary>
        /// Наводится на выбранные объекты
        /// </summary>
        private void ZoomTest()
        {
            if (ViewportName == null) return;
            var objectsID = Model.GetObjects();
            CadUtilityLib.ZoomToObjects(objectsID);
        }
        /// <summary>
        /// Приблизить на объекты в видовом экране
        /// </summary>
        private RelayCommand _ZoomCommand;
        public RelayCommand ZoomCommand
        {
            get
            {
                if (_ZoomCommand == null)
                {
                    _ZoomCommand = new RelayCommand(o => ZoomTest(), null);
                }
                return _ZoomCommand;
            }
        }

        /// <summary>
        /// Применить изменения
        /// </summary>
        private void Apply()
        {
            Model.ApplyParameters();
            //throw new System.Exception("Сделай применение изменений в макете!");
            OnPropertyChanged(nameof(ViewportName));
            OnPropertyChanged(nameof(Viewports));
            OnPropertyChanged(nameof(AnnotationScaleObjectsVP));
        }

        public void Update()
        {
            ManageViewport Current = Model.GetCurrentViewport();
            _ViewportName = Current.Name;
            _AnnotationScaleObjectsVP = Current.Scale;
        }

        private RelayCommand _ApplyCommand;
        public RelayCommand ApplyCommand
        {
            get
            {
                if (_ApplyCommand == null)
                {
                    _ApplyCommand = new RelayCommand(o => Apply(), null);
                }
                return _ApplyCommand;
            }
        }
    }
}
