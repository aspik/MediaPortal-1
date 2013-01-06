﻿using System;
using System.Globalization;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.EntityModel.Interfaces;
using Mediaportal.TV.Server.TVDatabase.EntityModel.Repositories;

namespace Mediaportal.TV.Server.TVDatabase.TVBusinessLayer
{
  public static class SettingsManagement
  {
    private static int _epgKeepDuration;
    public static int EpgKeepDuration
    {
      get
      {
        if (_epgKeepDuration == 0)
        {
          // first time query settings, caching
          _epgKeepDuration = GetValue("epgKeepDuration", 24);
        }
        return _epgKeepDuration;
      }
    }

    public static Setting GetSetting(string tagName)
    {
      using (ISettingsRepository settingsRepository = new SettingsRepository())
      {
        return settingsRepository.GetSetting(tagName);
      }
    }

    public static void SaveSetting(string tagName, string value)
    {
      using (ISettingsRepository settingsRepository = new SettingsRepository(true))
      {
        settingsRepository.SaveSetting(tagName, value);
      }
    }

    public static void SaveValue(string tagName, int defaultValue)
    {
      SaveSetting(tagName, defaultValue.ToString(CultureInfo.InvariantCulture));
    }
    
    public static void SaveValue(string tagName, double defaultValue)
    {
      SaveSetting(tagName, defaultValue.ToString(CultureInfo.InvariantCulture));
    }
    
    public static void SaveValue(string tagName, bool defaultValue)
    {
      SaveSetting(tagName, defaultValue.ToString(CultureInfo.InvariantCulture).ToLowerInvariant());
    }
    
    public static void SaveValue(string tagName, string defaultValue)
    {
      SaveSetting(tagName, defaultValue);
    }
    
    public static void SaveValue(string tagName, DateTime defaultValue)
    {
      SaveSetting(tagName, defaultValue.ToString(CultureInfo.InvariantCulture));
    }
    
    public static Setting GetSetting(string tagName, string defaultValue)
    {
      using (ISettingsRepository settingsRepository = new SettingsRepository(true))
      {
        return settingsRepository.GetOrSaveSetting(tagName, defaultValue);
      }
    }

    public static int GetValue(string tagName, int defaultValue)
    {
      Setting setting = GetSetting(tagName, defaultValue.ToString(CultureInfo.InvariantCulture));
      int number;
      bool parsed = int.TryParse(setting.Value, out number);
      if (!parsed)
      {
        number = defaultValue;
      }
      return number;
    }

    public static double GetValue(string tagName, double defaultValue)
    {
      Setting setting = GetSetting(tagName, defaultValue.ToString(CultureInfo.InvariantCulture));
      double number;      
      bool parsed = double.TryParse(setting.Value, out number);
      if (!parsed)
      {
        number = defaultValue;
      }
      return number;
    }

    public static bool GetValue(string tagName, bool defaultValue)
    {
      Setting setting = GetSetting(tagName, defaultValue.ToString(CultureInfo.InvariantCulture).ToLowerInvariant());
      return setting.Value == "true";
    }

    public static string GetValue(string tagName, string defaultValue)
    {
      Setting setting = GetSetting(tagName, defaultValue);
      return setting.Value;
    }

    public static DateTime GetValue(string tagName, DateTime defaultValue)
    {
      Setting setting = GetSetting(tagName, defaultValue.ToString(CultureInfo.InvariantCulture));
      return string.IsNullOrEmpty(setting.Value) ? DateTime.MinValue : DateTime.Parse(setting.Value, CultureInfo.InvariantCulture);
    }
  }
}