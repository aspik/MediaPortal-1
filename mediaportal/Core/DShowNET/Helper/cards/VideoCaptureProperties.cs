#region Copyright (C) 2005-2007 Team MediaPortal

/* 
 *	Copyright (C) 2005-2007 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#endregion

using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using MediaPortal.GUI.Library;
using DirectShowLib;
namespace DShowNET.Helper
{
    /// <summary>
    /// This class implements methods for vendor specific actions on tv capture cards.
    /// Currently we support
    /// - Hauppauge PVR cards
    /// - FireDTV digital cards
    /// </summary>
    public class VideoCaptureProperties : IDisposable
    {
        Twinhan _twinhan;
        Hauppauge _hauppauge;
        DigitalEverywhere _digitalEverywhere;
        TechnoTrend _technoTrend;
        public VideoCaptureProperties(IBaseFilter tunerfilter)
        {
          if (tunerfilter != null)
          {
            _twinhan = new Twinhan(tunerfilter);
            _hauppauge = new Hauppauge(tunerfilter);
            _digitalEverywhere = new DigitalEverywhere(tunerfilter);
            _technoTrend = new TechnoTrend(tunerfilter);
          }
         }

        #region Hauppauge Properties

        public void SetDNR(bool onoff)
        {
          _hauppauge.SetDNR(onoff);
        }

        public void SetAudioBitRate(int Kbps)
        {
              _hauppauge.SetAudioBitRate(Kbps);
              return;
        }

        public void SetVideoBitRate(int minKbps, int maxKbps, bool isVBR)
        {
                _hauppauge.SetVideoBitRate(minKbps, maxKbps, isVBR);
                return;
        }

        public bool GetVideoBitRate(out int minKbps, out int maxKbps, out bool isVBR)
        {
            minKbps = maxKbps = -1;
            isVBR = false;
            _hauppauge.GetVideoBitRate(out minKbps, out maxKbps, out isVBR);
            return true;
        }

      public bool GetAudioBitRate(out int audKbps)
      {
        audKbps = -1;
        _hauppauge.GetAudioBitRate(out audKbps);
        return true;
      }

      public void GetStreamType(out int stream)
      {
        _hauppauge.GetStream(out stream);
      }

      public void SetStreamType(int stream)
      {
        _hauppauge.SetStream(stream);
      }

     #endregion


      public bool IsCAPMTNeeded
      {
        get { return _twinhan.IsTwinhan; }
      }
        public bool SupportsCamSelection
      {
          get
          {
              if (_twinhan.IsTwinhan)
                  return true;
              return false;
          }
      }
        public bool SupportsHardwarePidFiltering
        {
            get
            {
                if (_digitalEverywhere.IsDigitalEverywhere)
                    return true;
                return false;
            }
        }

        public bool Supports5vAntennae
        {
            get
            {
                if (_technoTrend.IsTechnoTrendUSBDVBT)
                    return true;
                return false;
            }
        }

        public void EnableAntenna(bool onOff)
        {
            if (_technoTrend.IsTechnoTrendUSBDVBT)
            {
                _technoTrend.EnableAntenna(onOff);
            }
        }

        public bool SendPMT(string camType, int serviceId, int videoPid, int audioPid, byte[] PMT, int pmtLength, byte[] caPmt, int caPmtLen)
        {
            if (_digitalEverywhere.IsDigitalEverywhere)
            {
                return _digitalEverywhere.SendPMTToFireDTV(PMT, pmtLength);
            }
            if (_twinhan.IsTwinhan)
            {
                _twinhan.SendPMT(camType, (uint)videoPid, (uint)audioPid, caPmt, caPmtLen);
                return true;
            }
            if (_technoTrend.IsTechnoTrend)
            {
                return _technoTrend.SendPMT(serviceId);
            }
            return false;
        }

        public bool SetHardwarePidFiltering(bool isDvbc, bool isDvbT, bool isDvbS, bool isAtsc, ArrayList pids)
        {
            if (_digitalEverywhere.IsDigitalEverywhere)
            {
                return _digitalEverywhere.SetHardwarePidFiltering(isDvbc, isDvbT, isDvbS, isAtsc, pids);
            }
            return false;
        }

        public bool SupportsDiseqCommand()
        {
            if (_digitalEverywhere.IsDigitalEverywhere)
                return true;
            if (_technoTrend.IsTechnoTrend)
                return true;
            return false;
        }

        public void SendDiseqCommand(int antennaNr, int frequency, int switchingFrequency, int polarisation, int diseqcType)
        {
            if (_digitalEverywhere.IsDigitalEverywhere)
            {
                _digitalEverywhere.SendDiseqCommand(antennaNr, frequency, switchingFrequency, polarisation);
                return;
            }
            if (_technoTrend.IsTechnoTrend)
            {
                _technoTrend.SendDiseqCommand(antennaNr, frequency, switchingFrequency, polarisation, diseqcType);
                return;
            }
        }

        public bool IsCISupported()
        {
            if (_digitalEverywhere.IsDigitalEverywhere)
            {
                if (_digitalEverywhere.IsCamPresent())
                {
                    return true;
                }
                return false;
            }

            if (_twinhan.IsTwinhan)
            {
                if (_twinhan.IsCamPresent())
                {
                    return true;
                }
            }

            if (_technoTrend.IsTechnoTrend)
            {
                return _technoTrend.IsCamPresent();
            }
            return false;
        }

      
        public void Dispose()
        {
            _twinhan = null;
            if(_hauppauge != null)
              _hauppauge.Dispose();

            _hauppauge = null;
            _digitalEverywhere = null;
            if (_technoTrend != null)
            {
                _technoTrend.Dispose();
                _technoTrend = null;
            }
        }
    }//public class VideoCaptureProperties
}//namespace DShowNET
