/* 
 *  Copyright (C) 2006-2008 Team MediaPortal
 *  http://www.team-mediaportal.com
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
#pragma warning(disable : 4995)
#include <windows.h>
#include "..\shared\PacketSync.h"


CPacketSync::CPacketSync(void)
{
  m_tempBufferPos=-1;
}

CPacketSync::~CPacketSync(void)
{
}

void CPacketSync::Reset(void)
{
  m_tempBufferPos=-1;
}

// Ambass : Now, need to have 2 consecutive TS_PACKET_SYNC to try avoiding bad synchronisation.  
//          In case of data flow change ( Seek, tv Zap .... ) Reset() should be called first to flush buffer.
void CPacketSync::OnRawData(byte* pData, int nDataLen)
{
  int syncOffset=0;
  if (m_tempBufferPos > 0 )
  {
    if (pData[TS_PACKET_LEN - m_tempBufferPos]==TS_PACKET_SYNC)
    {
      syncOffset = TS_PACKET_LEN - m_tempBufferPos;
      if (syncOffset) memcpy(&m_tempBuffer[m_tempBufferPos], pData, syncOffset);
      OnTsPacket(m_tempBuffer);
    }
    m_tempBufferPos = 0;
  }

  while ((syncOffset + TS_PACKET_LEN) < nDataLen)
  {
    if ((pData[syncOffset] == TS_PACKET_SYNC) &&
        (pData[syncOffset + TS_PACKET_LEN]==TS_PACKET_SYNC))
    {
      OnTsPacket( &pData[syncOffset] );
      syncOffset += TS_PACKET_LEN;
    }
    else
      syncOffset++;
  }

  // Here we have less than 188+1 bytes
  while (syncOffset < nDataLen)
  {
    if (pData[syncOffset] == TS_PACKET_SYNC)
    {
      m_tempBufferPos= nDataLen - syncOffset;
      memcpy( m_tempBuffer, &pData[syncOffset], m_tempBufferPos );
      return ;
    }
    else
      syncOffset++;
  }

  m_tempBufferPos=0 ;
}

// Ambass : Now, need to have 2 consecutive TS_PACKET_SYNC to try avoiding bad synchronisation.  
//          In case of data flow change ( Seek, tv Zap .... ) Reset() should be called first to flush buffer.
// Owlsroost : This version will abandon a buffer if it fails to sync within 8 * TSpacket lengths
void CPacketSync::OnRawData2(byte* pData, int nDataLen)
{
  int syncOffset=0;
  int tempBuffOffset=0;
  bool goodPacket = false;

  if (m_tempBufferPos > 0 ) //We have some residual data from the last call
  {
    syncOffset = TS_PACKET_LEN - m_tempBufferPos;
    
    if (nDataLen <= syncOffset) 
    {
      //not enough total data to scan through a packet length, 
      //so add pData to the tempBuffer and return
      memcpy(&m_tempBuffer[m_tempBufferPos], pData, nDataLen);
      m_tempBufferPos += nDataLen;
      return ;
    }

    while ((nDataLen > syncOffset) && (m_tempBufferPos > tempBuffOffset)) 
    {
      if ((pData[syncOffset]==TS_PACKET_SYNC) &&
          (m_tempBuffer[tempBuffOffset]==TS_PACKET_SYNC)) //found a good packet
      {
        if (syncOffset) 
        {
          memcpy(&m_tempBuffer[m_tempBufferPos], pData, syncOffset);
        }
        OnTsPacket(&m_tempBuffer[tempBuffOffset]);
        goodPacket = true;
        break;
      }
      else
      {
        syncOffset++;
        tempBuffOffset++;
      }
    }    
    
    if (!goodPacket)
    {
      if (tempBuffOffset >= m_tempBufferPos)
      {
        //We have scanned all of the data in m_tempBuffer,
        //so continue search from the start of pData buffer.
        syncOffset = 0;
      }
      else
      {
        //move data down to discard data we have already scanned
        m_tempBufferPos -= tempBuffOffset;
        memmove(m_tempBuffer, &m_tempBuffer[tempBuffOffset], m_tempBufferPos);
        //add pData to the tempBuffer and return
        memcpy(&m_tempBuffer[m_tempBufferPos], pData, nDataLen);
        m_tempBufferPos += nDataLen;
        return;
      }
    }
  }

  m_tempBufferPos = 0; //We have consumed the residual data

  while (nDataLen > (syncOffset + TS_PACKET_LEN)) //minimum of TS_PACKET_LEN+1 bytes available
  {
    if (!goodPacket && (syncOffset > (TS_PACKET_LEN * 8)) )
    {
      //No sync - abandon the buffer
      m_tempBufferPos = -1;
      return;
    }
    if ((pData[syncOffset] == TS_PACKET_SYNC) &&
        (pData[syncOffset + TS_PACKET_LEN]==TS_PACKET_SYNC))
    {
      OnTsPacket( &pData[syncOffset] );
      syncOffset += TS_PACKET_LEN;
      goodPacket = true;
    }
    else
      syncOffset++;
  }
  
  // We have less than TS_PACKET_LEN+1 bytes available - store residual data for next time
  m_tempBufferPos= nDataLen - syncOffset;
  if (m_tempBufferPos)
  {
    memcpy( m_tempBuffer, &pData[syncOffset], m_tempBufferPos );
  }
}

void CPacketSync::OnTsPacket(byte* tsPacket)
{
}
