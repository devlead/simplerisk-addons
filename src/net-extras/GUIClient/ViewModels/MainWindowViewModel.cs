﻿using System;
using GUIClient.Configuration;
using GUIClient.Models;
using GUIClient.Services;
using Microsoft.Extensions.Localization;
using ReactiveUI;
using Splat;

namespace GUIClient.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        
        private bool _viewDashboardIsVisible = true;
        private bool _viewDeviceIsVisible = false;
        private bool _assessmentIsVisible = false;
        
        public string StrApplicationMN { get; }
        public string StrExitMN { get; }

        public bool ViewDashboardIsVisible
        {
            get => _viewDashboardIsVisible;
            set => this.RaiseAndSetIfChanged(ref _viewDashboardIsVisible, value);
        }
        
        public bool ViewDeviceIsVisible
        {
            get => _viewDeviceIsVisible;
            set => this.RaiseAndSetIfChanged(ref _viewDeviceIsVisible, value);
        }
        
        public bool AssessmentIsVisible
        {
            get => _assessmentIsVisible;
            set => this.RaiseAndSetIfChanged(ref _assessmentIsVisible, value);
        }
        
        private DeviceViewModel _deviceViewModel = 
            new DeviceViewModel();

        public DeviceViewModel DeviceViewModel
        {
            get => _deviceViewModel;
            set =>  this.RaiseAndSetIfChanged(ref _deviceViewModel, value);
        }

        public MainWindowViewModel(ILocalizationService localizationService)
        {
            
            StrApplicationMN = Localizer["ApplicationMN"];
            StrExitMN = Localizer["ExitMN"];
        }

        public void OnMenuExitCommand()
        {
            Environment.Exit(0);
        }

        public void NavigateTo(AvaliableViews view)
        {
            HideAllViews();
            switch (view)
            {
                case AvaliableViews.Dashboard:
                    ViewDashboardIsVisible = true;
                    break;
                case AvaliableViews.Devices:
                    DeviceViewModel.Initialize();
                    ViewDeviceIsVisible = true;
                    break;
                case AvaliableViews.Assessment:
                    
                    AssessmentIsVisible = true;
                    break;
            }
        }

        private void HideAllViews()
        {
            ViewDashboardIsVisible = false;
            ViewDeviceIsVisible = false;
            AssessmentIsVisible = false;
        }
        
        
    }
}