﻿/*
 * Copyright (c) 2009, openmetaverse.org
 * All rights reserved.
 *
 * - Redistribution and use in source and binary forms, with or without 
 *   modification, are permitted provided that the following conditions are met:
 *
 * - Redistributions of source code must retain the above copyright notice, this
 *   list of conditions and the following disclaimer.
 * - Neither the name of the openmetaverse.org nor the names 
 *   of its contributors may be used to endorse or promote products derived from
 *   this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" 
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE 
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE 
 * LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR 
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF 
 * SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS 
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN 
 * CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE 
 * POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Collections.Generic;
using System.Net;
using OpenMetaverse.StructuredData;
using OpenMetaverse.Interfaces;

namespace OpenMetaverse.Messages.Linden
{
    #region Teleport/Region/Movement Messages

    /// <summary>
    /// 
    /// </summary>
    public class TeleportFinishMessage : IMessage
    {
        public UUID AgentID;
        public int LocationID;
        public ulong RegionHandle;
        public Uri SeedCapability;
        public SimAccess SimAccess;
        public IPAddress IP;
        public int Port;
        public TeleportFlags Flags;

        public OSDMap Serialize()
        {

            OSDMap map = new OSDMap(1);

            OSDArray infoArray = new OSDArray(1);

            OSDMap info = new OSDMap(8);
            info.Add("AgentID", OSD.FromUUID(AgentID));
            info.Add("LocationID", OSD.FromInteger(LocationID)); // Unused by the client
            info.Add("RegionHandle", OSD.FromULong(RegionHandle));
            info.Add("SeedCapability", OSD.FromUri(SeedCapability));
            info.Add("SimAccess", OSD.FromInteger((byte)SimAccess));
            info.Add("SimIP", MessageUtils.FromIP(IP));
            info.Add("SimPort", OSD.FromInteger(Port));
            info.Add("TeleportFlags", OSD.FromUInteger((uint)Flags));

            infoArray.Add(info);

            map.Add("Info", infoArray);

            return map;
        }

        public void Deserialize(OSDMap map)
        {
            Console.WriteLine("Deserializing TeleportFinish Message");
            OSDArray array = (OSDArray)map["Info"];
            OSDMap blockMap = (OSDMap)array[0];

            AgentID = blockMap["AgentID"].AsUUID();
            LocationID = blockMap["LocationID"].AsInteger();
            RegionHandle = blockMap["RegionHandle"].AsULong();
            SeedCapability = blockMap["SeedCapability"].AsUri();
            SimAccess = (SimAccess)blockMap["SimAccess"].AsInteger();
            IP = MessageUtils.ToIP(blockMap["SimIP"]);
            Port = blockMap["SimPort"].AsInteger();
            Flags = (TeleportFlags)blockMap["TeleportFlags"].AsUInteger();
        }
    }

    public class EstablishAgentCommunicationMessage : IMessage
    {
        public UUID AgentID;
        public IPAddress Address;
        public int Port;
        public Uri SeedCapability;

        public OSDMap Serialize()
        {
            OSDMap map = new OSDMap(3);
            map["agent-id"] = OSD.FromUUID(AgentID);
            map["sim-ip-and-port"] = OSD.FromString(String.Format("{0}:{1}", Address, Port));
            map["seed-capability"] = OSD.FromUri(SeedCapability);
            return map;
        }

        public void Deserialize(OSDMap map)
        {
            string ipAndPort = map["sim-ip-and-port"].AsString();
            int i = ipAndPort.IndexOf(':');

            AgentID = map["agent-id"].AsUUID();
            Address = IPAddress.Parse(ipAndPort.Substring(0, i));
            Port = Int32.Parse(ipAndPort.Substring(i + 1));
            SeedCapability = map["seed-capability"].AsUri();
        }
    }

    public class CrossedRegionMessage
    {
        public Vector3 LookAt;
        public Vector3 Position;
        public UUID AgentID;
        public UUID SessionID;
        public ulong RegionHandle;
        public Uri SeedCapability;
        public IPAddress IP;
        public int Port;

        public OSDMap Serialize()
        {
            OSDMap map = new OSDMap(3);

            OSDArray infoArray = new OSDArray(1);
            OSDMap infoMap = new OSDMap(2);
            infoMap["LookAt"] = OSD.FromVector3(LookAt);
            infoMap["Position"] = OSD.FromVector3(Position);
            infoArray.Add(infoMap);
            map["Info"] = infoArray;

            OSDArray agentDataArray = new OSDArray(1);
            OSDMap agentDataMap = new OSDMap(2);
            agentDataMap["AgentID"] = OSD.FromUUID(AgentID);
            agentDataMap["SessionID"] = OSD.FromUUID(SessionID);
            agentDataArray.Add(agentDataMap);
            map["AgentData"] = agentDataArray;

            OSDArray regionDataArray = new OSDArray(1);
            OSDMap regionDataMap = new OSDMap(4);
            regionDataMap["RegionHandle"] = OSD.FromULong(RegionHandle);
            regionDataMap["SeedCapability"] = OSD.FromUri(SeedCapability);
            regionDataMap["SimIP"] = MessageUtils.FromIP(IP);
            regionDataMap["SimPort"] = OSD.FromInteger(Port);
            regionDataArray.Add(regionDataMap);
            map["RegionData"] = regionDataArray;

            return map;
        }

        public void Deserialize(OSDMap map)
        {
            OSDMap infoMap = (OSDMap)((OSDArray)map["Info"])[0];
            LookAt = infoMap["LookAt"].AsVector3();
            Position = infoMap["Position"].AsVector3();

            OSDMap agentDataMap = (OSDMap)((OSDArray)map["AgentData"])[0];
            AgentID = agentDataMap["AgentID"].AsUUID();
            SessionID = agentDataMap["SessionID"].AsUUID();

            OSDMap regionDataMap = (OSDMap)((OSDArray)map["RegionData"])[0];
            RegionHandle = regionDataMap["RegionHandle"].AsULong();
            SeedCapability = regionDataMap["SeedCapability"].AsUri();
            IP = MessageUtils.ToIP(regionDataMap["SimIP"]);
            Port = regionDataMap["SimPort"].AsInteger();
        }
    }

    public class EnableSimulatorMessage : IMessage
    {
        public class SimulatorInfoBlock
        {
            public ulong RegionHandle;
            public IPAddress IP;
            public int Port;
        }

        public SimulatorInfoBlock[] Simulators;

        public OSDMap Serialize()
        {
            OSDMap map = new OSDMap(1);

            OSDArray array = new OSDArray(Simulators.Length);
            for (int i = 0; i < Simulators.Length; i++)
            {
                SimulatorInfoBlock block = Simulators[i];

                OSDMap blockMap = new OSDMap(3);
                blockMap["Handle"] = OSD.FromULong(block.RegionHandle);
                blockMap["IP"] = MessageUtils.FromIP(block.IP);
                blockMap["Port"] = OSD.FromInteger(block.Port);
                array.Add(blockMap);
            }

            map["SimulatorInfo"] = array;
            return map;
        }

        public void Deserialize(OSDMap map)
        {
            OSDArray array = (OSDArray)map["SimulatorInfo"];
            Simulators = new SimulatorInfoBlock[array.Count];

            for (int i = 0; i < array.Count; i++)
            {
                OSDMap blockMap = (OSDMap)array[i];

                SimulatorInfoBlock block = new SimulatorInfoBlock();
                block.RegionHandle = blockMap["Handle"].AsULong();
                block.IP = MessageUtils.ToIP(blockMap["IP"]);
                block.Port = blockMap["Port"].AsInteger();
                Simulators[i] = block;
            }
        }
    }

    public class LandStatReplyMessage : IMessage
    {

        /* Single map
         'RequestData':
        [        
            {
            'ReportType':b64"AAAAAA=="
            ,
            'RequestFlags':b64"AAAABA=="
            ,
            'TotalObjectCount':b64"AAABbw=="
            }
        ]
         */
        public int ReporType;
        public int RequestFlags;
        public int TotalObjectCount;

        /*
          'DataExtended':
            [        
                {
                'MonoScore':r0.0053327744826674461
                ,
                'TimeStamp':b64"Seo9lw=="
                }
            ]
            ,
         'ReportData':
            [        
                {
                'LocationX':r34.764884948730469
                ,
                'LocationY':r86.75262451171875
                ,
                'LocationZ':r26.555828094482422
                ,
                'OwnerName':'Preostan Scribe'
                ,
                'Score':r0.0023237180430442095
                ,
                'TaskID':u1623b11b-127f-a170-da37-21523b9967a1
                ,
                'TaskLocalID':b64"BhZW4g=="
                ,
                'TaskName':'Dutch Door Upper Half'
                }
            ]
      ,*/
        public class ReportDataBlock
        {
            public Vector3 Location;
            public string OwnerName;
            public float Score;
            public UUID TaskID;
            public uint TaskLocalID;
            public string TaskName;
            public float MonoScore;
            public DateTime TimeStamp;
        }

        public ReportDataBlock[] ReportDataBlocks;

        public OSDMap Serialize()
        {
            OSDMap map = new OSDMap(3);

            OSDMap requestDataMap = new OSDMap(3);
            requestDataMap["ReportType"] = OSD.FromInteger(this.ReporType);
            requestDataMap["RequestFlags"] = OSD.FromInteger(this.RequestFlags);
            requestDataMap["TotalObjectCount"] = OSD.FromInteger(this.TotalObjectCount);
            map["RequestData"] = requestDataMap;

            OSDArray reportDataArray = new OSDArray();
            OSDArray dataExtendedArray = new OSDArray();
            for (int i = 0; i < ReportDataBlocks.Length; i++)
            {
                OSDMap reportMap = new OSDMap(8);
                reportMap["LocationX"] = OSD.FromReal(ReportDataBlocks[i].Location.X);
                reportMap["LocationY"] = OSD.FromReal(ReportDataBlocks[i].Location.Y);
                reportMap["LocationZ"] = OSD.FromReal(ReportDataBlocks[i].Location.Z);
                reportMap["OwnerName"] = OSD.FromString(ReportDataBlocks[i].OwnerName);
                reportMap["Score"] = OSD.FromReal(ReportDataBlocks[i].Score);
                reportMap["TaskID"] = OSD.FromUUID(ReportDataBlocks[i].TaskID);
                reportMap["TaskLocalID"] = OSD.FromReal(ReportDataBlocks[i].TaskLocalID);
                reportMap["TaskName"] = OSD.FromString(ReportDataBlocks[i].TaskName);
                reportDataArray.Add(reportMap);

                OSDMap extendedMap = new OSDMap(2);
                extendedMap["MonoScore"] = OSD.FromReal(ReportDataBlocks[i].MonoScore);
                extendedMap["TimeStamp"] = OSD.FromDate(ReportDataBlocks[i].TimeStamp);
                dataExtendedArray.Add(extendedMap);
            }

            map["ReportData"] = reportDataArray;
            map["ExtendedData"] = dataExtendedArray;

            return map;
        }

        public void Deserialize(OSDMap map)
        {
            
            OSDMap requestDataMap = (OSDMap)map["RequestData"];
            this.ReporType = requestDataMap["ReportType"].AsInteger();
            this.RequestFlags = requestDataMap["RequestFlags"].AsInteger();
            this.TotalObjectCount = requestDataMap["TotalObjectCount"].AsInteger();

            OSDArray dataArray = (OSDArray)map["ReportData"];
            OSDArray dataExtendedArray = (OSDArray)map["ExtendedData"];

            ReportDataBlocks = new ReportDataBlock[dataArray.Count];
            for (int i = 0; i < dataArray.Count; i++)
            {
                OSDMap blockMap = (OSDMap)dataArray[i];
                OSDMap extMap = (OSDMap)dataExtendedArray[i];
                ReportDataBlock block = new ReportDataBlock();
                block.Location = new Vector3(
                    (float)blockMap["LocationX"].AsReal(),
                    (float)blockMap["LocationY"].AsReal(),
                    (float)blockMap["LocationZ"].AsReal());
                block.OwnerName = blockMap["OwnerName"].AsString();
                block.Score = (float)blockMap["Score"].AsReal();
                block.TaskID = blockMap["TaskID"].AsUUID();
                block.TaskLocalID = blockMap["TaskLocalID"].AsUInteger();
                block.TaskName = blockMap["TaskName"].AsString();
                block.MonoScore = (float)extMap["MonoScore"].AsReal();
                block.TimeStamp = extMap["TimeStamp"].AsDate();

                ReportDataBlocks[i] = block;
            }
        }
    }

    #endregion

    #region Parcel Messages

    /// <summary>
    /// Contains a list of prim owner information for a specific parcel in a simulator
    /// </summary>
    /// <remarks>
    /// A Simulator will always return at least 1 entry
    /// If agent does not have proper permission the OwnerID will be UUID.Zero
    /// If agent does not have proper permission OR there are no primitives on parcel
    /// the DataBlocksExtended map will not be sent from the simulator
    /// </remarks>
    public class ParcelObjectOwnersReplyMessage : IMessage
    {
        /// <summary>
        /// Prim ownership information for a specified owner on a single parcel
        /// </summary>
        public class PrimOwners
        {
            /// <summary>The <see cref="UUID"/> of the prim owner, 
            /// UUID.Zero if agent has no permission</summary>
            public UUID OwnerID;
            /// <summary>The total number of prims on parcel owned</summary>
            public int Count;
            /// <summary>True if the Owner is a group</summary>
            public bool IsGroupOwned;
            /// <summary>True if the owner is online 
            /// <remarks>This is no longer used by the LL Simulators</remarks></summary>
            public bool OnlineStatus;
            /// <summary>The date of the newest prim</summary>
            public DateTime TimeStamp;
        }

        /// <summary>
        /// An Array of Datablocks containing prim owner information
        /// </summary>
        public PrimOwners[] DataBlocks;

        /// <summary>
        /// Create an OSDMap from the strongly typed message
        /// </summary>
        /// <returns></returns>
        public OSDMap Serialize()
        {
            OSDArray dataArray = new OSDArray(DataBlocks.Length);
            OSDArray dataExtendedArray = new OSDArray();

            for (int i = 0; i < DataBlocks.Length; i++)
            {
                OSDMap dataMap = new OSDMap(4);
                dataMap["OwnerID"] = OSD.FromUUID(DataBlocks[i].OwnerID);
                dataMap["Count"] = OSD.FromInteger(DataBlocks[i].Count);
                dataMap["IsGroupOwned"] = OSD.FromBoolean(DataBlocks[i].IsGroupOwned);
                dataMap["OnlineStatus"] = OSD.FromBoolean(DataBlocks[i].OnlineStatus);
                dataArray.Add(dataMap);

                /* If the tmestamp is null, don't create the DataExtended map, this 
                 * is usually when the parcel contains no primitives, or the agent does not have
                 * permissions to see ownership information */
                if (DataBlocks[i].TimeStamp != null)
                {
                    OSDMap dataExtendedMap = new OSDMap(1);
                    dataExtendedMap["TimeStamp"] = OSD.FromDate(DataBlocks[i].TimeStamp);
                    dataExtendedArray.Add(dataExtendedMap);
                }
            }

            OSDMap map = new OSDMap();
            map.Add("Data", dataArray);
            if (dataExtendedArray.Count > 0)
                map.Add("DataExtended", dataExtendedArray);

            return map;
        }

        /// <summary>
        /// Convert an OSDMap into the a strongly typed object containing 
        /// prim ownership information
        /// </summary>
        /// <param name="map"></param>
        public void Deserialize(OSDMap map)
        {
            OSDArray dataArray = (OSDArray)map["Data"];

            // DataExtended is optional, will not exist of parcel contains zero prims
            OSDArray dataExtendedArray;
            if (map.ContainsKey("DataExtended"))
            {
                dataExtendedArray = (OSDArray)map["DataExtended"];
            }
            else
            {
                dataExtendedArray = new OSDArray();
            }

            DataBlocks = new PrimOwners[dataArray.Count];

            for (int i = 0; i < dataArray.Count; i++)
            {
                OSDMap dataMap = (OSDMap)dataArray[i];
                PrimOwners block = new PrimOwners();
                block.OwnerID = dataMap["OwnerID"].AsUUID();
                block.Count = dataMap["Count"].AsInteger();
                block.IsGroupOwned = dataMap["IsGroupOwned"].AsBoolean();
                block.OnlineStatus = dataMap["OnlineStatus"].AsBoolean(); // deprecated

                /* if the agent has no permissions, or there are no prims, the counts
                 * should not match up, so we don't decode the DataExtended map */
                if (dataExtendedArray.Count == dataArray.Count)
                {
                    OSDMap dataExtendedMap = (OSDMap)dataExtendedArray[i];
                    block.TimeStamp = Utils.UnixTimeToDateTime(dataExtendedMap["TimeStamp"].AsUInteger());
                }

                DataBlocks[i] = block;
            }
        }
    }

    /// <summary>
    /// The details of a single parcel in a region, also contains some regionwide globals
    /// </summary>
    public class ParcelPropertiesMessage : IMessage
    {
        /// <summary>Simulator-local ID of this parcel</summary>
        public int LocalID;
        /// <summary>Maximum corner of the axis-aligned bounding box for this
        /// parcel</summary>
        public Vector3 AABBMax;
        /// <summary>Minimum corner of the axis-aligned bounding box for this
        /// parcel</summary>
        public Vector3 AABBMin;
        /// <summary>Total parcel land area</summary>
        public int Area;
        /// <summary></summary>
        public uint AuctionID;
        /// <summary>Key of authorized buyer</summary>
        public UUID AuthBuyerID;
        /// <summary>Bitmap describing land layout in 4x4m squares across the 
        /// entire region</summary>
        public byte[] Bitmap;
        /// <summary></summary>
        public ParcelCategory Category;
        /// <summary>Date land was claimed</summary>
        public DateTime ClaimDate;
        /// <summary>Appears to always be zero</summary>
        public int ClaimPrice;
        /// <summary>Parcel Description</summary>
        public string Desc;
        /// <summary></summary>
        public ParcelFlags ParcelFlags;
        /// <summary></summary>
        public UUID GroupID;
        /// <summary>Total number of primitives owned by the parcel group on 
        /// this parcel</summary>
        public int GroupPrims;
        /// <summary>Whether the land is deeded to a group or not</summary>
        public bool IsGroupOwned;
        /// <summary></summary>
        public LandingType LandingType;
        /// <summary>Maximum number of primitives this parcel supports</summary>
        public int MaxPrims;
        /// <summary>The Asset UUID of the Texture which when applied to a 
        /// primitive will display the media</summary>
        public UUID MediaID;
        /// <summary>A URL which points to any Quicktime supported media type</summary>
        public string MediaURL;
        /// <summary>A byte, if 0x1 viewer should auto scale media to fit object</summary>
        public bool MediaAutoScale;
        /// <summary>URL For Music Stream</summary>
        public string MusicURL;
        /// <summary>Parcel Name</summary>
        public string Name;
        /// <summary>Autoreturn value in minutes for others' objects</summary>
        public int OtherCleanTime;
        /// <summary></summary>
        public int OtherCount;
        /// <summary>Total number of other primitives on this parcel</summary>
        public int OtherPrims;
        /// <summary>UUID of the owner of this parcel</summary>
        public UUID OwnerID;
        /// <summary>Total number of primitives owned by the parcel owner on 
        /// this parcel</summary>
        public int OwnerPrims;
        /// <summary></summary>
        public float ParcelPrimBonus;
        /// <summary>How long is pass valid for</summary>
        public float PassHours;
        /// <summary>Price for a temporary pass</summary>
        public int PassPrice;
        /// <summary></summary>
        public int PublicCount;
        /// <summary></summary>
        public bool RegionDenyAnonymous;
        /// <summary></summary>
        public bool RegionPushOverride;
        /// <summary>This field is no longer used</summary>
        public int RentPrice;
        /// The result of a request for parcel properties
        public ParcelResult RequestResult;
        /// <summary>Sale price of the parcel, only useful if ForSale is set</summary>
        /// <remarks>The SalePrice will remain the same after an ownership
        /// transfer (sale), so it can be used to see the purchase price after
        /// a sale if the new owner has not changed it</remarks>
        public int SalePrice;
        /// <summary>
        /// Number of primitives your avatar is currently
        /// selecting and sitting on in this parcel
        /// </summary>
        public int SelectedPrims;
        /// <summary></summary>
        public int SelfCount;
        /// <summary>
        /// A number which increments by 1, starting at 0 for each ParcelProperties request. 
        /// Can be overriden by specifying the sequenceID with the ParcelPropertiesRequest being sent. 
        /// a Negative number indicates the action in <seealso cref="ParcelPropertiesStatus"/> has occurred. 
        /// </summary>
        public int SequenceID;
        /// <summary>Maximum primitives across the entire simulator</summary>
        public int SimWideMaxPrims;
        /// <summary>Total primitives across the entire simulator</summary>
        public int SimWideTotalPrims;
        /// <summary></summary>
        public bool SnapSelection;
        /// <summary>Key of parcel snapshot</summary>
        public UUID SnapshotID;
        /// <summary>Parcel ownership status</summary>
        public ParcelStatus Status;
        /// <summary>Total number of primitives on this parcel</summary>
        public int TotalPrims;
        /// <summary></summary>
        public Vector3 UserLocation;
        /// <summary></summary>
        public Vector3 UserLookAt;
        /// <summary>TRUE of region denies access to age unverified users</summary>
        public bool RegionDenyAgeUnverified;
        /// <summary>A description of the media</summary>
        public string MediaDesc;
        /// <summary>An Integer which represents the height of the media</summary>
        public int MediaHeight;
        /// <summary>An integer which represents the width of the media</summary>
        public int MediaWidth;
        /// <summary>A boolean, if true the viewer should loop the media</summary>
        public bool MediaLoop;
        /// <summary>A string which contains the mime type of the media</summary>
        public string MediaType;
        /// <summary>true to obscure (hide) media url</summary>
        public bool ObscureMedia;
        /// <summary>true to obscure (hide) music url</summary>
        public bool ObscureMusic;

        /// <summary>
        /// Encode the data object as an OSDMap
        /// </summary>
        /// <returns>An OSDMap containing the encoded object</returns>
        public OSDMap Serialize()
        {

            OSDMap map = new OSDMap(3);

            OSDArray dataArray = new OSDArray(1);
            OSDMap parcelDataMap = new OSDMap(47);
            parcelDataMap["LocalID"] = OSD.FromInteger(LocalID);
            parcelDataMap["AABBMax"] = OSD.FromVector3(AABBMax);
            parcelDataMap["AABBMin"] = OSD.FromVector3(AABBMin);
            parcelDataMap["Area"] = OSD.FromInteger(Area);
            parcelDataMap["AuctionID"] = OSD.FromInteger(AuctionID);
            parcelDataMap["AuthBuyerID"] = OSD.FromUUID(AuthBuyerID);
            parcelDataMap["Bitmap"] = OSD.FromBinary(Bitmap);
            parcelDataMap["Category"] = OSD.FromInteger((int)Category);
            parcelDataMap["ClaimDate"] = OSD.FromDate(ClaimDate);
            parcelDataMap["ClaimPrice"] = OSD.FromInteger(ClaimPrice);
            parcelDataMap["Desc"] = OSD.FromString(Desc);
            //parcelDataMap["ParcelFlags"] = OSD.FromLong((long)ParcelFlags); // verify this!
            parcelDataMap["ParcelFlags"] = OSD.FromUInteger((uint)ParcelFlags);
            parcelDataMap["GroupID"] = OSD.FromUUID(GroupID);
            parcelDataMap["GroupPrims"] = OSD.FromInteger(GroupPrims);
            parcelDataMap["IsGroupOwned"] = OSD.FromBoolean(IsGroupOwned);
            parcelDataMap["LandingType"] = OSD.FromInteger((int)LandingType);
            parcelDataMap["MaxPrims"] = OSD.FromInteger(MaxPrims);
            parcelDataMap["MediaID"] = OSD.FromUUID(MediaID);
            parcelDataMap["MediaURL"] = OSD.FromString(MediaURL);
            parcelDataMap["MediaAutoScale"] = OSD.FromBoolean(MediaAutoScale);
            parcelDataMap["MusicURL"] = OSD.FromString(MusicURL);
            parcelDataMap["Name"] = OSD.FromString(Name);
            parcelDataMap["OtherCleanTime"] = OSD.FromInteger(OtherCleanTime);
            parcelDataMap["OtherCount"] = OSD.FromInteger(OtherCount);
            parcelDataMap["OtherPrims"] = OSD.FromInteger(OtherPrims);
            parcelDataMap["OwnerID"] = OSD.FromUUID(OwnerID);
            parcelDataMap["OwnerPrims"] = OSD.FromInteger(OwnerPrims);
            parcelDataMap["ParcelPrimBonus"] = OSD.FromReal((float)ParcelPrimBonus);
            parcelDataMap["PassHours"] = OSD.FromReal((float)PassHours);
            parcelDataMap["PassPrice"] = OSD.FromInteger(PassPrice);
            parcelDataMap["PublicCount"] = OSD.FromInteger(PublicCount);
            parcelDataMap["RegionDenyAnonymous"] = OSD.FromBoolean(RegionDenyAnonymous);
            parcelDataMap["RegionPushOverride"] = OSD.FromBoolean(RegionPushOverride);
            parcelDataMap["RentPrice"] = OSD.FromInteger(RentPrice);
            parcelDataMap["RequestResult"] = OSD.FromInteger((int)RequestResult);
            parcelDataMap["SalePrice"] = OSD.FromInteger(SalePrice);
            parcelDataMap["SelectedPrims"] = OSD.FromInteger(SelectedPrims);
            parcelDataMap["SelfCount"] = OSD.FromInteger(SelfCount);
            parcelDataMap["SequenceID"] = OSD.FromInteger(SequenceID);
            parcelDataMap["SimWideMaxPrims"] = OSD.FromInteger(SimWideMaxPrims);
            parcelDataMap["SimWideTotalPrims"] = OSD.FromInteger(SimWideTotalPrims);
            parcelDataMap["SnapSelection"] = OSD.FromBoolean(SnapSelection);
            parcelDataMap["SnapshotID"] = OSD.FromUUID(SnapshotID);
            parcelDataMap["Status"] = OSD.FromInteger((int)Status);
            parcelDataMap["TotalPrims"] = OSD.FromInteger(TotalPrims);
            parcelDataMap["UserLocation"] = OSD.FromVector3(UserLocation);
            parcelDataMap["UserLookAt"] = OSD.FromVector3(UserLookAt);
            dataArray.Add(parcelDataMap);
            map["ParcelData"] = dataArray;

            OSDArray mediaDataArray = new OSDArray(1);
            OSDMap mediaDataMap = new OSDMap(7);
            mediaDataMap["MediaDesc"] = OSD.FromString(MediaDesc);
            mediaDataMap["MediaHeight"] = OSD.FromInteger(MediaHeight);
            mediaDataMap["MediaWidth"] = OSD.FromInteger(MediaWidth);
            mediaDataMap["MediaLoop"] = OSD.FromBoolean(MediaLoop);
            mediaDataMap["MediaType"] = OSD.FromString(MediaType);
            mediaDataMap["ObscureMedia"] = OSD.FromBoolean(ObscureMedia);
            mediaDataMap["ObscureMusic"] = OSD.FromBoolean(ObscureMusic);
            mediaDataArray.Add(mediaDataMap);
            map["MediaData"] = mediaDataArray;

            OSDArray ageVerificationBlockArray = new OSDArray(1);
            OSDMap ageVerificationBlockMap = new OSDMap(1);
            ageVerificationBlockMap["RegionDenyAgeUnverified"] = OSD.FromBoolean(RegionDenyAgeUnverified);
            ageVerificationBlockArray.Add(ageVerificationBlockMap);
            map["AgeVerificationBlock"] = ageVerificationBlockArray;

            return map;
        }

        public void Deserialize(OSDMap map)
        {
            OSDMap parcelDataMap = (OSDMap)((OSDArray)map["ParcelData"])[0];
            LocalID = parcelDataMap["LocalID"].AsInteger();
            AABBMax = parcelDataMap["AABBMax"].AsVector3();
            AABBMin = parcelDataMap["AABBMin"].AsVector3();
            Area = parcelDataMap["Area"].AsInteger();
            AuctionID = (uint)parcelDataMap["AuctionID"].AsInteger();
            AuthBuyerID = parcelDataMap["AuthBuyerID"].AsUUID();
            Bitmap = parcelDataMap["Bitmap"].AsBinary();
            Category = (ParcelCategory)parcelDataMap["Category"].AsInteger();
            ClaimDate = Utils.UnixTimeToDateTime((uint)parcelDataMap["ClaimDate"].AsInteger());
            ClaimPrice = parcelDataMap["ClaimPrice"].AsInteger();
            Desc = parcelDataMap["Desc"].AsString();

            // LL sends this as binary, we'll convert it here
            if (parcelDataMap["ParcelFlags"].Type == OSDType.Binary)
            {
                byte[] bytes = parcelDataMap["ParcelFlags"].AsBinary();
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(bytes);
                ParcelFlags = (ParcelFlags)BitConverter.ToUInt32(bytes, 0);
            }
            else
            {
                ParcelFlags = (ParcelFlags)parcelDataMap["ParcelFlags"].AsUInteger();
            }
            GroupID = parcelDataMap["GroupID"].AsUUID();
            GroupPrims = parcelDataMap["GroupPrims"].AsInteger();
            IsGroupOwned = parcelDataMap["IsGroupOwned"].AsBoolean();
            LandingType = (LandingType)parcelDataMap["LandingType"].AsInteger();
            MaxPrims = parcelDataMap["MaxPrims"].AsInteger();
            MediaID = parcelDataMap["MediaID"].AsUUID();
            MediaURL = parcelDataMap["MediaURL"].AsString();
            MediaAutoScale = parcelDataMap["MediaAutoScale"].AsBoolean(); // 0x1 = yes
            MusicURL = parcelDataMap["MusicURL"].AsString();
            Name = parcelDataMap["Name"].AsString();
            OtherCleanTime = parcelDataMap["OtherCleanTime"].AsInteger();
            OtherCount = parcelDataMap["OtherCount"].AsInteger();
            OtherPrims = parcelDataMap["OtherPrims"].AsInteger();
            OwnerID = parcelDataMap["OwnerID"].AsUUID();
            OwnerPrims = parcelDataMap["OwnerPrims"].AsInteger();
            ParcelPrimBonus = (float)parcelDataMap["ParcelPrimBonus"].AsReal();
            PassHours = (float)parcelDataMap["PassHours"].AsReal();
            PassPrice = parcelDataMap["PassPrice"].AsInteger();
            PublicCount = parcelDataMap["PublicCount"].AsInteger();
            RegionDenyAnonymous = parcelDataMap["RegionDenyAnonymous"].AsBoolean();
            RegionPushOverride = parcelDataMap["RegionPushOverride"].AsBoolean();
            RentPrice = parcelDataMap["RentPrice"].AsInteger();
            RequestResult = (ParcelResult)parcelDataMap["RequestResult"].AsInteger();
            SalePrice = parcelDataMap["SalePrice"].AsInteger();
            SelectedPrims = parcelDataMap["SelectedPrims"].AsInteger();
            SelfCount = parcelDataMap["SelfCount"].AsInteger();
            SequenceID = parcelDataMap["SequenceID"].AsInteger();
            SimWideMaxPrims = parcelDataMap["SimWideMaxPrims"].AsInteger();
            SimWideTotalPrims = parcelDataMap["SimWideTotalPrims"].AsInteger();
            SnapSelection = parcelDataMap["SnapSelection"].AsBoolean();
            SnapshotID = parcelDataMap["SnapshotID"].AsUUID();
            Status = (ParcelStatus)parcelDataMap["Status"].AsInteger();
            TotalPrims = parcelDataMap["TotalPrims"].AsInteger();
            UserLocation = parcelDataMap["UserLocation"].AsVector3();
            UserLookAt = parcelDataMap["UserLookAt"].AsVector3();

            OSDMap mediaDataMap = (OSDMap)((OSDArray)map["MediaData"])[0];
            MediaDesc = mediaDataMap["MediaDesc"].AsString();
            MediaHeight = mediaDataMap["MediaHeight"].AsInteger();
            MediaWidth = mediaDataMap["MediaWidth"].AsInteger();
            MediaLoop = mediaDataMap["MediaLoop"].AsBoolean();
            MediaType = mediaDataMap["MediaType"].AsString();
            ObscureMedia = mediaDataMap["ObscureMedia"].AsBoolean();
            ObscureMusic = mediaDataMap["ObscureMusic"].AsBoolean();

            OSDMap ageVerificationBlockMap = (OSDMap)((OSDArray)map["AgeVerificationBlock"])[0];
            RegionDenyAgeUnverified = ageVerificationBlockMap["RegionDenyAgeUnverified"].AsBoolean();
        }
    }

    public class ParcelPropertiesUpdateMessage : IMessage
    {
        public UUID AuthBuyerID;
        public bool MediaAutoScale;
        public ParcelCategory Category;
        public string Desc;
        public UUID GroupID;
        public LandingType Landing;
        public int LocalID;
        public string MediaDesc;
        public int MediaHeight;
        public bool MediaLoop;
        public UUID MediaID;
        public string MediaType;
        public string MediaURL;
        public int MediaWidth;
        public string MusicURL;
        public string Name;
        public bool ObscureMedia;
        public bool ObscureMusic;
        public ParcelFlags ParcelFlags;
        public float PassHours;
        public uint PassPrice;
        public uint SalePrice;
        public UUID SnapshotID;
        public Vector3 UserLocation;
        public Vector3 UserLookAt;

        public void Deserialize(OSDMap map)
        {
            AuthBuyerID = map["auth_buyer_id"].AsUUID();
            MediaAutoScale = map["auto_scale"].AsBoolean();
            Category = (ParcelCategory)map["category"].AsInteger();
            Desc = map["description"].AsString();
            GroupID = map["group_id"].AsUUID();
            Landing = (LandingType)map["landing_type"].AsUInteger();
            LocalID = map["local_id"].AsInteger();
            MediaDesc = map["media_desc"].AsString();
            MediaHeight = map["media_height"].AsInteger();
            MediaLoop = map["media_loop"].AsBoolean();
            MediaID = map["media_id"].AsUUID();
            MediaType = map["media_type"].AsString();
            MediaURL = map["media_url"].AsString();
            MediaWidth = map["media_width"].AsInteger();
            MusicURL = map["music_url"].AsString();
            Name = map["name"].AsString();
            ObscureMedia = map["obscure_media"].AsBoolean();
            ObscureMusic = map["obscure_music"].AsBoolean();
            ParcelFlags = (ParcelFlags)map["parcel_flags"].AsUInteger();
            PassHours = (float)map["pass_hours"].AsReal();
            PassPrice = map["pass_price"].AsUInteger();
            SalePrice = map["sale_price"].AsUInteger();
            SnapshotID = map["snapshot_id"].AsUUID();
            UserLocation = map["user_location"].AsVector3();
            UserLookAt = map["user_look_at"].AsVector3();
        }

        public OSDMap Serialize()
        {
            OSDMap map = new OSDMap();
            map["auth_buyer_id"] = OSD.FromUUID(AuthBuyerID);
            map["auto_scale"] = OSD.FromBoolean(MediaAutoScale);
            map["category"] = OSD.FromInteger((byte)Category);
            map["description"] = OSD.FromString(Desc);
            map["flags"] = OSD.FromBinary(Utils.EmptyBytes);
            map["group_id"] = OSD.FromUUID(GroupID);
            map["landing_type"] = OSD.FromInteger((byte)Landing);
            map["local_id"] = OSD.FromInteger(LocalID);
            map["media_desc"] = OSD.FromString(MediaDesc);
            map["media_height"] = OSD.FromInteger(MediaHeight);
            map["media_id"] = OSD.FromUUID(MediaID);
            map["media_loop"] = OSD.FromBoolean(MediaLoop);
            map["media_type"] = OSD.FromString(MediaType);
            map["media_url"] = OSD.FromString(MediaURL);
            map["media_width"] = OSD.FromInteger(MediaWidth);
            map["music_url"] = OSD.FromString(MusicURL);
            map["name"] = OSD.FromString(Name);
            map["obscure_media"] = OSD.FromBoolean(ObscureMedia);
            map["obscure_music"] = OSD.FromBoolean(ObscureMusic);
            map["parcel_flags"] = OSD.FromUInteger((uint)ParcelFlags);
            map["pass_hours"] = OSD.FromReal(PassHours);
            map["pass_price"] = OSD.FromInteger(PassPrice);
            map["sale_price"] = OSD.FromInteger(SalePrice);
            map["snapshot_id"] = OSD.FromUUID(SnapshotID);
            map["user_location"] = OSD.FromVector3(UserLocation);
            map["user_look_at"] = OSD.FromVector3(UserLookAt);

            return map;
        }
    }
    public class RemoteParcelRequestMessage : IMessage
    {
        public UUID ParcelID;

        public OSDMap Serialize()
        {
            OSDMap map = new OSDMap(1);
            map["parcel_id"] = OSD.FromUUID(ParcelID);
            return map;
        }

        public void Deserialize(OSDMap map)
        {
            ParcelID = map["parcel_id"].AsUUID();
        }

    }
    #endregion

    #region Inventory Messages

    public class NewFileAgentInventoryMessage
    {
        public UUID FolderID;
        public AssetType AssetType;
        public InventoryType InventoryType;
        public string Name;
        public string Description;

        public OSDMap Serialize()
        {
            OSDMap map = new OSDMap(5);
            map["folder_id"] = OSD.FromUUID(FolderID);
            map["asset_type"] = OSD.FromString(Utils.AssetTypeToString(AssetType));
            map["inventory_type"] = OSD.FromString(Utils.InventoryTypeToString(InventoryType));
            map["name"] = OSD.FromString(Name);
            map["description"] = OSD.FromString(Description);

            return map;

        }

        public void Deserialize(OSDMap map)
        {
            FolderID = map["folder_id"].AsUUID();
            AssetType = Utils.StringToAssetType(map["asset_type"].AsString());
            InventoryType = Utils.StringToInventoryType(map["inventory_type"].AsString());
            Name = map["name"].AsString();
            Description = map["description"].AsString();
        }
    }

    #endregion

    #region Agent Messages

    public class AgentGroupDataUpdateMessage : IMessage
    {
        public UUID AgentID;

        public class GroupData
        {
            public bool AcceptNotices;
            public int Contribution;
            public UUID GroupID;
            public UUID GroupInsigniaID;
            public string GroupName;
            public GroupPowers GroupPowers;
            public bool ListInProfile;
        }

        public GroupData[] GroupDataBlock;

        public OSDMap Serialize()
        {
            OSDMap map = new OSDMap(3);

            OSDMap agent = new OSDMap(1);
            agent["AgentID"] = OSD.FromUUID(AgentID);

            OSDArray agentArray = new OSDArray();
            agentArray.Add(agent);

            map["AgentData"] = agentArray;

            OSDArray groupDataArray = new OSDArray(GroupDataBlock.Length);

            for (int i = 0; i < GroupDataBlock.Length; i++)
            {
                OSDMap group = new OSDMap(7);
                group["AcceptNotices"] = OSD.FromBoolean(GroupDataBlock[i].AcceptNotices);
                group["Contribution"] = OSD.FromInteger(GroupDataBlock[i].Contribution);
                group["GroupID"] = OSD.FromUUID(GroupDataBlock[i].GroupID);
                group["GroupInsigniaID"] = OSD.FromUUID(GroupDataBlock[i].GroupInsigniaID);
                group["GroupName"] = OSD.FromString(GroupDataBlock[i].GroupName);
                group["GroupPowers"] = OSD.FromLong((long)GroupDataBlock[i].GroupPowers);
                group["ListInProfile"] = OSD.FromBoolean(GroupDataBlock[i].ListInProfile);
                groupDataArray.Add(group);
            }

            map["GroupData"] = groupDataArray;

            return map;
        }

        public void Deserialize(OSDMap map)
        {
            OSDArray agentArray = (OSDArray)map["AgentData"];
            OSDMap agentMap = (OSDMap)agentArray[0];
            AgentID = agentMap["AgentID"].AsUUID();

            OSDArray groupArray = (OSDArray)map["GroupData"];

            //OSDArray newGroupDataArray = (OSDArray)map["NewGroupData"];

            GroupDataBlock = new GroupData[groupArray.Count];

            for (int i = 0; i < groupArray.Count; i++)
            {
                OSDMap groupMap = (OSDMap)groupArray[i];


                GroupData groupData = new GroupData();

                groupData.GroupID = groupMap["GroupID"].AsUUID();
                groupData.Contribution = groupMap["Contribution"].AsInteger();
                groupData.GroupInsigniaID = groupMap["GroupInsigniaID"].AsUUID();
                groupData.GroupName = groupMap["GroupName"].AsString();
                groupData.GroupPowers = (GroupPowers)groupMap["GroupPowers"].AsLong();
                groupData.ListInProfile = groupMap["ListInProfile"].AsBoolean();
                groupData.AcceptNotices = groupMap["AcceptNotices"].AsBoolean();
                GroupDataBlock[i] = groupData;
            }
        }
    }

    public class UpdateAgentLanguageMessage : IMessage
    {
        public string Language;
        public bool LanguagePublic;

        public OSDMap Serialize()
        {
            OSDMap map = new OSDMap(2);

            map["language"] = OSD.FromString(Language);
            map["language_is_public"] = OSD.FromBoolean(LanguagePublic);

            return map;
        }

        public void Deserialize(OSDMap map)
        {
            LanguagePublic = map["language_is_public"].AsBoolean();
            Language = map["language"].AsString();
        }

    }

    #endregion

    #region Voice Messages
    public class RequiredVoiceVersionMessage : IMessage
    {
        public int MajorVersion;
        public int MinorVersion;
        public string RegionName;
        public string Message = "RequiredVoiceVersion";

        public OSDMap Serialize()
        {
            OSDMap map = new OSDMap(4);
            map["major_version"] = OSD.FromInteger(MajorVersion);
            map["minor_version"] = OSD.FromInteger(MinorVersion);
            map["region_name"] = OSD.FromString(RegionName);
            map["message"] = OSD.FromString(Message);
            return map;
        }

        public void Deserialize(OSDMap map)
        {
            MajorVersion = map["major_version"].AsInteger();
            MinorVersion = map["minor_version"].AsInteger();
            RegionName = map["region_name"].AsString();
            Message = map["message"].AsString();
        }
    }

    public class ParcelVoiceInfoRequestMessage : IMessage
    {
        public int ParcelID;
        public string RegionName;
        public Uri SipChannelUri;

        public OSDMap Serialize()
        {
            OSDMap map = new OSDMap(3);
            map["parcel_local_id"] = OSD.FromInteger(ParcelID);
            map["region_name"] = OSD.FromString(RegionName);

            OSDMap vcMap = new OSDMap(1);
            vcMap["channel_uri"] = OSD.FromUri(SipChannelUri);

            map["voice_credentials"] = vcMap;

            return map;
        }

        public void Deserialize(OSDMap map)
        {
            ParcelID = map["parcel_local_id"].AsInteger();
            RegionName = map["region_name"].AsString();

            OSDMap vcMap = (OSDMap)map["voice_credentials"];
            SipChannelUri = vcMap["channel_uri"].AsUri();
        }
    }

    public class ProvisionVoiceAccountRequestMessage : IMessage
    {
        public string Password;
        public string Username;

        public OSDMap Serialize()
        {
            OSDMap map = new OSDMap(2);

            map["username"] = OSD.FromString(Username);
            map["password"] = OSD.FromString(Password);

            return map;
        }

        public void Deserialize(OSDMap map)
        {
            Username = map["username"].AsString();
            Password = map["password"].AsString();
        }
    }

    #endregion

    #region Script/Notecards Messages
    // upload a script to a tasks inventory
    public class UploadScriptTaskMessage : IMessage
    {
        public string State; // "upload"
        public Uri UploaderUrl;

        public OSDMap Serialize()
        {
            OSDMap map = new OSDMap(2);
            map["state"] = OSD.FromString(State);
            map["uploader"] = OSD.FromUri(UploaderUrl);

            return map;
        }

        public void Deserialize(OSDMap map)
        {
            State = map["state"].AsString();
            UploaderUrl = map["uploader"].AsUri();
        }
    }

    public class ScriptRunningReplyMessage : IMessage
    {
        public UUID ItemID;
        public bool Mono;
        public UUID ObjectID;
        public bool Running;
        public string message = "ScriptRunningReply";

        public OSDMap Serialize()
        {
            OSDMap map = new OSDMap(2);

            OSDMap scriptMap = new OSDMap(4);
            scriptMap["ItemID"] = OSD.FromUUID(ItemID);
            scriptMap["Mono"] = OSD.FromBoolean(Mono);
            scriptMap["ObjectID"] = OSD.FromUUID(ObjectID);
            scriptMap["Running"] = OSD.FromBoolean(Running);

            OSDArray scriptArray = new OSDArray(1);
            scriptArray.Add((OSD)scriptMap);

            map["Script"] = scriptArray;
            map["message"] = OSD.FromString(message);

            return map;
        }

        public void Deserialize(OSDMap map)
        {
            OSDArray scriptArray = (OSDArray)map["Script"];

            OSDMap scriptMap = (OSDMap)scriptArray[0];

            ItemID = scriptMap["ItemID"].AsUUID();
            Mono = scriptMap["Mono"].AsBoolean();
            ObjectID = scriptMap["ObjectID"].AsUUID();
            Running = scriptMap["Running"].AsBoolean();
            message = map["message"].AsString();

        }
    }

    public class UpdateNotecardTaskInventoryMessage
    {
        public UUID TaskID;
        public UUID ItemID;

        public OSDMap Serialize()
        {
            OSDMap map = new OSDMap(1);
            map["task_id"] = OSD.FromUUID(TaskID);
            map["item_id"] = OSD.FromUUID(ItemID);

            return map;
        }

        public void Deserialize(OSDMap map)
        {
            TaskID = map["task_id"].AsUUID();
            ItemID = map["item_id"].AsUUID();
        }
    }

    // TODO: Add Test
    public class UpdateNotecardAgentInventoryMessage : IMessage
    {
        public UUID ItemID;

        public OSDMap Serialize()
        {
            OSDMap map = new OSDMap(1);
            map["item_id"] = OSD.FromUUID(ItemID);

            return map;
        }

        public void Deserialize(OSDMap map)
        {
            ItemID = map["item_id"].AsUUID();
        }
    }

    public class CopyInventoryFromNotecardMessage : IMessage
    {
        public int CallbackID;
        public UUID FolderID;
        public UUID ItemID;
        public UUID NotecardID;
        public UUID ObjectID;

        public OSDMap Serialize()
        {
            OSDMap map = new OSDMap(5);
            map["callback-id"] = OSD.FromInteger(CallbackID);
            map["folder-id"] = OSD.FromUUID(FolderID);
            map["item-id"] = OSD.FromUUID(ItemID);
            map["notecard-id"] = OSD.FromUUID(NotecardID);
            map["object-id"] = OSD.FromUUID(ObjectID);

            return map;
        }

        public void Deserialize(OSDMap map)
        {
            CallbackID = map["callback-id"].AsInteger();
            FolderID = map["folder-id"].AsUUID();
            ItemID = map["item-id"].AsUUID();
            NotecardID = map["notecard-id"].AsUUID();
            ObjectID = map["object-id"].AsUUID();
        }
    }

    /// <summary>
    /// Request sent by client to update a script inside a tasks inventory
    /// </summary>
    public class UpdateScriptTaskMessage : IMessage
    {
        public bool ScriptRunning;
        public UUID ItemID;
        public string Target; // mono or lsl2
        public UUID TaskID;

        public OSDMap Serialize()
        {
            OSDMap map = new OSDMap(4);
            map["is_script_running"] = OSD.FromBoolean(ScriptRunning);
            map["item_id"] = OSD.FromUUID(ItemID);
            map["target"] = OSD.FromString(Target);
            map["task_id"] = OSD.FromUUID(TaskID);
            return map;
        }

        public void Deserialize(OSDMap map)
        {
            ScriptRunning = map["is_script_running"].AsBoolean();
            ItemID = map["item_id"].AsUUID();
            Target = map["target"].AsString();
            TaskID = map["task_id"].AsUUID();
        }
    }

    public class UpdateScriptAgentMessage : IMessage
    {
        public UUID ItemID;
        public string Target;

        public OSDMap Serialize()
        {
            OSDMap map = new OSDMap(2);
            map["item_id"] = OSD.FromUUID(ItemID);
            map["target"] = OSD.FromString(Target);
            return map;
        }

        public void Deserialize(OSDMap map)
        {
            ItemID = map["item_id"].AsUUID();
            Target = map["target"].AsString();
        }

    }

    public class SendPostcardMessage : IMessage
    {
        public string FromEmail;
        public string Message;
        public string FromName;
        public Vector3 GlobalPosition;
        public string Subject;
        public string ToEmail;

        public OSDMap Serialize()
        {
            OSDMap map = new OSDMap(6);
            map["from"] = OSD.FromString(FromEmail);
            map["msg"] = OSD.FromString(Message);
            map["name"] = OSD.FromString(FromName);
            map["pos-global"] = OSD.FromVector3(GlobalPosition);
            map["subject"] = OSD.FromString(Subject);
            map["to"] = OSD.FromString(ToEmail);
            return map;
        }

        public void Deserialize(OSDMap map)
        {
            FromEmail = map["from"].AsString();
            Message = map["msg"].AsString();
            FromName = map["name"].AsString();
            GlobalPosition = map["pos-global"].AsVector3();
            Subject = map["subject"].AsString();
            ToEmail = map["to"].AsString();
        }
    }

    #endregion

    #region Grid/Maps

    public class MapLayerMessage : IMessage
    {
        // AgentData -> Flags
        public int Flags;

        public class LayerData
        {
            public UUID ImageID;
            public int Bottom;
            public int Left;
            public int Right;
            public int Top;
        }

        public LayerData[] LayerDataBlocks;

        public OSDMap Serialize()
        {
            OSDMap map = new OSDMap(2);
            OSDMap agentMap = new OSDMap(1);
            agentMap["Flags"] = OSD.FromInteger(Flags);
            map["AgentData"] = agentMap;

            OSDArray layerArray = new OSDArray(LayerDataBlocks.Length);

            for (int i = 0; i < LayerDataBlocks.Length; i++)
            {
                OSDMap layerMap = new OSDMap(5);
                layerMap["ImageID"] = OSD.FromUUID(LayerDataBlocks[i].ImageID);
                layerMap["Bottom"] = OSD.FromInteger(LayerDataBlocks[i].Bottom);
                layerMap["Left"] = OSD.FromInteger(LayerDataBlocks[i].Left);
                layerMap["Top"] = OSD.FromInteger(LayerDataBlocks[i].Top);
                layerMap["Right"] = OSD.FromInteger(LayerDataBlocks[i].Right);

                layerArray.Add(layerMap);
            }

            map["LayerData"] = layerArray;

            return map;
        }

        public void Deserialize(OSDMap map)
        {
            OSDMap agentMap = (OSDMap)map["AgentData"];
            Flags = agentMap["Flags"].AsInteger();

            OSDArray layerArray = (OSDArray)map["LayerData"];

            LayerDataBlocks = new LayerData[layerArray.Count];

            for (int i = 0; i < LayerDataBlocks.Length; i++)
            {
                OSDMap layerMap = (OSDMap)layerArray[i];

                LayerData layer = new LayerData();
                layer.ImageID = layerMap["ImageID"].AsUUID();
                layer.Top = layerMap["Top"].AsInteger();
                layer.Right = layerMap["Right"].AsInteger();
                layer.Left = layerMap["Left"].AsInteger();
                layer.Bottom = layerMap["Bottom"].AsInteger();

                LayerDataBlocks[i] = layer;
            }
        }
    }

    #endregion

    #region Session/Communication


    public class ChatSessionRequestMessage : IMessage
    {

        /// <summary>
        /// The Session ID
        /// </summary>
        public UUID SessionID;
        /// <summary>
        /// The method used to update session, currently known valid values:
        ///  mute update
        /// </summary>
        public string Method;

        public UUID AgentID;
        /// <summary>
        /// A list containing Key/Value pairs, known valid values:
        /// key: text value: true/false - allow/disallow specified agents ability to use text in session
        /// key: voice value: true/false - allow/disallow specified agents ability to use voice in session
        /// </summary>
        public string RequestKey;
        public bool RequestValue;



        public OSDMap Serialize()
        {
            OSDMap map = new OSDMap(3);
            map["method"] = OSD.FromString(Method);

            OSDMap muteMap = new OSDMap(1);
            muteMap[RequestKey] = OSD.FromBoolean(RequestValue);

            OSDMap paramMap = new OSDMap(2);
            paramMap["agent_id"] = OSD.FromUUID(AgentID);
            paramMap["mute_info"] = muteMap;

            map["params"] = paramMap;
            map["session-id"] = OSD.FromUUID(SessionID);

            return map;
        }

        public void Deserialize(OSDMap map)
        {
            Method = map["method"].AsString();
            SessionID = map["session-id"].AsUUID();

            OSDMap paramsMap = (OSDMap)map["params"];
            OSDMap muteMap = (OSDMap)paramsMap["mute_info"];

            AgentID = paramsMap["agent_id"].AsUUID();

            if (muteMap.ContainsKey("text"))
                RequestKey = "text";
            else if (muteMap.ContainsKey("voice"))
                RequestKey = "voice";

            RequestValue = muteMap[RequestKey].AsBoolean();
        }
    }

    public class ChatterboxSessionEventReplyMessage : IMessage
    {
        public UUID SessionID;
        public bool Success;

        public OSDMap Serialize()
        {
            OSDMap map = new OSDMap(2);
            map["success"] = OSD.FromBoolean(Success);
            map["session_id"] = OSD.FromUUID(SessionID); // FIXME: Verify this is correct map name

            return map;
        }

        public void Deserialize(OSDMap map)
        {
            Success = map["success"].AsBoolean();
            SessionID = map["session_id"].AsUUID();
        }
    }

    public class ChatterBoxSessionStartReplyMessage : IMessage
    {
        public UUID SessionID;
        public UUID TempSessionID;
        public bool Success;

        public string SessionName;
        // FIXME: Replace int with an enum
        public int Type;
        public bool VoiceEnabled;
        public bool ModeratedVoice;

        /* Is Text moderation possible? */

        public OSDMap Serialize()
        {
            OSDMap moderatedMap = new OSDMap(1);
            moderatedMap["voice"] = OSD.FromBoolean(ModeratedVoice);

            OSDMap sessionMap = new OSDMap(4);
            sessionMap["type"] = OSD.FromInteger(Type);
            sessionMap["session_name"] = OSD.FromString(SessionName);
            sessionMap["voice_enabled"] = OSD.FromBoolean(VoiceEnabled);
            sessionMap["moderated_mode"] = moderatedMap;

            OSDMap map = new OSDMap(4);
            map["session_id"] = OSD.FromUUID(SessionID);
            map["temp_session_id"] = OSD.FromUUID(TempSessionID);
            map["success"] = OSD.FromBoolean(Success);
            map["session_info"] = sessionMap;

            return map;
        }

        public void Deserialize(OSDMap map)
        {
            SessionID = map["session_id"].AsUUID();
            TempSessionID = map["temp_session_id"].AsUUID();
            Success = map["success"].AsBoolean();

            if (Success)
            {
                OSDMap sessionMap = (OSDMap)map["session_info"];
                SessionName = sessionMap["session_name"].AsString();
                Type = sessionMap["type"].AsInteger();
                VoiceEnabled = sessionMap["voice_enabled"].AsBoolean();

                OSDMap moderatedModeMap = (OSDMap)sessionMap["moderated_mode"];
                ModeratedVoice = moderatedModeMap["voice"].AsBoolean();
            }
        }
    }

    public class ChatterBoxInvitationMessage : IMessage
    {
        /// <summary>Key of sender</summary>
        public UUID FromAgentID;
        /// <summary>Name of sender</summary>
        public string FromAgentName;
        /// <summary>Key of destination avatar</summary>
        public UUID ToAgentID;
        /// <summary>ID of originating estate</summary>
        public uint ParentEstateID;
        /// <summary>Key of originating region</summary>
        public UUID RegionID;
        /// <summary>Coordinates in originating region</summary>
        public Vector3 Position;
        /// <summary>Instant message type</summary>
        public InstantMessageDialog Dialog;
        /// <summary>Group IM session toggle</summary>
        public bool GroupIM;
        /// <summary>Key of IM session, for Group Messages, the groups UUID</summary>
        public UUID IMSessionID;
        /// <summary>Timestamp of the instant message</summary>
        public DateTime Timestamp;
        /// <summary>Instant message text</summary>
        public string Message;
        /// <summary>Whether this message is held for offline avatars</summary>
        public InstantMessageOnline Offline;
        /// <summary>Context specific packed data</summary>
        public byte[] BinaryBucket;

        public OSDMap Serialize()
        {
            OSDMap dataMap = new OSDMap(3);
            dataMap["timestamp"] = OSD.FromDate(Timestamp);
            dataMap["type"] = OSD.FromInteger((uint)Dialog);
            dataMap["binary_bucket"] = OSD.FromBinary(BinaryBucket);

            OSDMap paramsMap = new OSDMap(11);
            paramsMap["from_id"] = OSD.FromUUID(FromAgentID);
            paramsMap["from_name"] = OSD.FromString(FromAgentName);
            paramsMap["to_id"] = OSD.FromUUID(ToAgentID);
            paramsMap["parent_estate_id"] = OSD.FromInteger(ParentEstateID);
            paramsMap["region_id"] = OSD.FromUUID(RegionID);
            paramsMap["position"] = OSD.FromVector3(Position);
            paramsMap["from_group"] = OSD.FromBoolean(GroupIM);
            paramsMap["session_id"] = OSD.FromUUID(IMSessionID);
            paramsMap["message"] = OSD.FromString(Message);
            paramsMap["offline"] = OSD.FromInteger((uint)Offline);

            paramsMap["data"] = dataMap;

            OSDMap imMap = new OSDMap(1);
            imMap["message_params"] = paramsMap;

            OSDMap map = new OSDMap(1);
            map["instantmessage"] = imMap;


            return map;
        }

        public void Deserialize(OSDMap map)
        {
            OSDMap im = (OSDMap)map["instantmessage"];
            OSDMap msg = (OSDMap)im["message_params"];
            OSDMap msgdata = (OSDMap)msg["data"];

            FromAgentID = msg["from_id"].AsUUID();
            FromAgentName = msg["from_name"].AsString();
            ToAgentID = msg["to_id"].AsUUID();
            ParentEstateID = (uint)msg["parent_estate_id"].AsInteger();
            RegionID = msg["region_id"].AsUUID();
            Position = msg["position"].AsVector3();
            GroupIM = msg["from_group"].AsBoolean();
            IMSessionID = msg["session_id"].AsUUID();
            Message = msg["message"].AsString();
            Offline = (InstantMessageOnline)msg["offline"].AsInteger();
            Dialog = (InstantMessageDialog)msgdata["type"].AsInteger();
            BinaryBucket = msgdata["binary_bucket"].AsBinary();
            Timestamp = msgdata["timestamp"].AsDate();
        }
    }

    /// <summary>
    /// Sent from the simulator to the viewer.
    /// 
    /// When an agent initially joins a session the AgentUpdatesBlock object will contain a list of session members including
    /// a boolean indicating they can use voice chat in this session, a boolean indicating they are allowed to moderate 
    /// this session, and lastly a string which indicates another agent is entering the session with the Transition set to "ENTER"
    /// 
    /// During the session lifetime updates on individuals are sent. During the update the booleans sent during the initial join are
    /// excluded with the exception of the Transition field. This indicates a new user entering or exiting the session with
    /// the string "ENTER" or "LEAVE" respectively.
    /// </summary>
    public class ChatterBoxSessionAgentListUpdatesMessage : IMessage
    {
        // initial when agent joins session
        // <llsd><map><key>events</key><array><map><key>body</key><map><key>agent_updates</key><map><key>32939971-a520-4b52-8ca5-6085d0e39933</key><map><key>info</key><map><key>can_voice_chat</key><boolean>1</boolean><key>is_moderator</key><boolean>1</boolean></map><key>transition</key><string>ENTER</string></map><key>ca00e3e1-0fdb-4136-8ed4-0aab739b29e8</key><map><key>info</key><map><key>can_voice_chat</key><boolean>1</boolean><key>is_moderator</key><boolean>0</boolean></map><key>transition</key><string>ENTER</string></map></map><key>session_id</key><string>be7a1def-bd8a-5043-5d5b-49e3805adf6b</string><key>updates</key><map><key>32939971-a520-4b52-8ca5-6085d0e39933</key><string>ENTER</string><key>ca00e3e1-0fdb-4136-8ed4-0aab739b29e8</key><string>ENTER</string></map></map><key>message</key><string>ChatterBoxSessionAgentListUpdates</string></map><map><key>body</key><map><key>agent_updates</key><map><key>32939971-a520-4b52-8ca5-6085d0e39933</key><map><key>info</key><map><key>can_voice_chat</key><boolean>1</boolean><key>is_moderator</key><boolean>1</boolean></map></map></map><key>session_id</key><string>be7a1def-bd8a-5043-5d5b-49e3805adf6b</string><key>updates</key><map /></map><key>message</key><string>ChatterBoxSessionAgentListUpdates</string></map></array><key>id</key><integer>5</integer></map></llsd>

        // a message containing only moderator updates
        // <llsd><map><key>events</key><array><map><key>body</key><map><key>agent_updates</key><map><key>ca00e3e1-0fdb-4136-8ed4-0aab739b29e8</key><map><key>info</key><map><key>mutes</key><map><key>text</key><boolean>1</boolean></map></map></map></map><key>session_id</key><string>be7a1def-bd8a-5043-5d5b-49e3805adf6b</string><key>updates</key><map /></map><key>message</key><string>ChatterBoxSessionAgentListUpdates</string></map></array><key>id</key><integer>7</integer></map></llsd>

        public UUID SessionID;
        public string Message = "ChatterBoxSessionAgentListUpdates"; // message

        public class AgentUpdatesBlock
        {
            public UUID AgentID;

            public bool CanVoiceChat;
            public bool IsModerator;
            // transition "transition" = "ENTER" or "LEAVE"
            public string Transition;   //  TODO: switch to an enum "ENTER" or "LEAVE"

            public bool MuteText;
            public bool MuteVoice;
        }

        public AgentUpdatesBlock[] Updates;

        public OSDMap Serialize()
        {
            OSDMap map = new OSDMap();

            OSDMap agent_updatesMap = new OSDMap(1);
            for (int i = 0; i < Updates.Length; i++)
            {
                OSDMap mutesMap = new OSDMap(2);
                mutesMap["text"] = OSD.FromBoolean(Updates[i].MuteText);
                mutesMap["voice"] = OSD.FromBoolean(Updates[i].MuteVoice);

                OSDMap infoMap = new OSDMap(4);
                infoMap["can_voice_chat"] = OSD.FromBoolean((bool)Updates[i].CanVoiceChat);
                infoMap["is_moderator"] = OSD.FromBoolean((bool)Updates[i].IsModerator);
                infoMap["transition"] = OSD.FromString(Updates[i].Transition);

                OSDMap imap = new OSDMap(1);
                imap["info"] = infoMap;
                imap.Add("mutes", mutesMap);

                agent_updatesMap.Add(Updates[i].AgentID.ToString(), imap);
            }

            map.Add("agent_updates", agent_updatesMap);

            OSDMap updates = new OSDMap();

            map["session_id"] = OSD.FromUUID(SessionID);

            map["message"] = OSD.FromString(Message);

            return map;
        }

        public void Deserialize(OSDMap map)
        {

            OSDMap agent_updates = (OSDMap)map["agent_updates"];
            SessionID = map["session_id"].AsUUID();
            Message = map["message"].AsString();

            List<AgentUpdatesBlock> updatesList = new List<AgentUpdatesBlock>();

            foreach (KeyValuePair<string, OSD> kvp in agent_updates)
            {

                if (kvp.Key == "updates")
                {
                    // This appears to be redundant and duplicated by the info block, more dumps will confirm this
                    /* <key>32939971-a520-4b52-8ca5-6085d0e39933</key>
                            <string>ENTER</string> */
                }
                else if (kvp.Key == "session_id")
                {
                    // I am making the assumption that each osdmap will contain the information for a 
                    // single session. This is how the map appears to read however more dumps should be taken
                    // to confirm this.
                    /* <key>session_id</key>
                            <string>984f6a1e-4ceb-6366-8d5e-a18c6819c6f7</string> */

                }
                else  // key is an agent uuid (we hope!)
                {
                    // should be the agents uuid as the key, and "info" as the datablock
                    /* <key>32939971-a520-4b52-8ca5-6085d0e39933</key>
                            <map>
                                <key>info</key>
                                    <map>
                                        <key>can_voice_chat</key>
                                            <boolean>1</boolean>
                                        <key>is_moderator</key>
                                            <boolean>1</boolean>
                                    </map>
                                <key>transition</key>
                                    <string>ENTER</string>
                            </map>*/
                    AgentUpdatesBlock block = new AgentUpdatesBlock();
                    block.AgentID = UUID.Parse(kvp.Key);

                    OSDMap infoMap = (OSDMap)agent_updates[kvp.Key];

                    OSDMap agentPermsMap = (OSDMap)infoMap["info"];

                    block.CanVoiceChat = agentPermsMap["can_voice_chat"].AsBoolean();
                    block.IsModerator = agentPermsMap["is_moderator"].AsBoolean();
                    block.Transition = agentPermsMap["transition"].AsString();

                    if (infoMap.ContainsKey("mutes"))
                    {
                        OSDMap mutesMap = (OSDMap)infoMap["mutes"];
                        block.MuteText = mutesMap["text"].AsBoolean();
                        block.MuteVoice = mutesMap["voice"].AsBoolean();
                    }
                    updatesList.Add(block);
                }

            }

            Updates = new AgentUpdatesBlock[updatesList.Count];

            for (int i = 0; i < updatesList.Count; i++)
            {
                AgentUpdatesBlock block = new AgentUpdatesBlock();
                block.AgentID = updatesList[i].AgentID;
                block.CanVoiceChat = updatesList[i].CanVoiceChat;
                block.IsModerator = updatesList[i].IsModerator;
                block.MuteText = updatesList[i].MuteText;
                block.MuteVoice = updatesList[i].MuteVoice;
                block.Transition = updatesList[i].Transition;
                Updates[i] = block;
            }
        }
    }

    #endregion

    #region Stats Messages

    public class ViewerStatsMessage : IMessage
    {
        public int AgentsInView;
        public float AgentFPS;
        public string AgentLanguage;
        public float AgentMemoryUsed;
        public float MetersTraveled;
        public float AgentPing;
        public int RegionsVisited;
        public float AgentRuntime;
        public float SimulatorFPS;
        public DateTime AgentStartTime;
        public string AgentVersion;

        public float object_kbytes;
        public float texture_kbytes;
        public float world_kbytes;

        public float MiscVersion;
        public bool VertexBuffersEnabled;

        public UUID SessionID;

        public int StatsDropped;
        public int StatsFailedResends;
        public int FailuresInvalid;
        public int FailuresOffCircuit;
        public int FailuresResent;
        public int FailuresSendPacket;

        public int MiscInt1;
        public int MiscInt2;
        public string MiscString1;

        public int InCompressedPackets;
        public float InKbytes;
        public float InPackets;
        public float InSavings;

        public int OutCompressedPackets;
        public float OutKbytes;
        public float OutPackets;
        public float OutSavings;

        public string SystemCPU;
        public string SystemGPU;
        public int SystemGPUClass;
        public string SystemGPUVendor;
        public string SystemGPUVersion;
        public string SystemOS;
        public int SystemInstalledRam;

        public OSDMap Serialize()
        {
            OSDMap map = new OSDMap(5);
            map["session_id"] = OSD.FromUUID(SessionID);

            OSDMap agentMap = new OSDMap(11);
            agentMap["agents_in_view"] = OSD.FromInteger(AgentsInView);
            agentMap["fps"] = OSD.FromReal(AgentFPS);
            agentMap["language"] = OSD.FromString(AgentLanguage);
            agentMap["mem_use"] = OSD.FromReal(AgentMemoryUsed);
            agentMap["meters_traveled"] = OSD.FromReal(MetersTraveled);
            agentMap["ping"] = OSD.FromReal(AgentPing);
            agentMap["regions_visited"] = OSD.FromInteger(RegionsVisited);
            agentMap["run_time"] = OSD.FromReal(AgentRuntime);
            agentMap["sim_fps"] = OSD.FromReal(SimulatorFPS);
            agentMap["start_time"] = OSD.FromUInteger(Utils.DateTimeToUnixTime(AgentStartTime));
            agentMap["version"] = OSD.FromString(AgentVersion);
            map["agent"] = agentMap;


            OSDMap downloadsMap = new OSDMap(3); // downloads
            downloadsMap["object_kbytes"] = OSD.FromReal(object_kbytes);
            downloadsMap["texture_kbytes"] = OSD.FromReal(texture_kbytes);
            downloadsMap["world_kbytes"] = OSD.FromReal(world_kbytes);
            map["downloads"] = downloadsMap;

            OSDMap miscMap = new OSDMap(2);
            miscMap["Version"] = OSD.FromReal(MiscVersion);
            miscMap["Vertex Buffers Enabled"] = OSD.FromBoolean(VertexBuffersEnabled);
            map["misc"] = miscMap;

            OSDMap statsMap = new OSDMap(2);

            OSDMap failuresMap = new OSDMap(6);
            failuresMap["dropped"] = OSD.FromInteger(StatsDropped);
            failuresMap["failed_resends"] = OSD.FromInteger(StatsFailedResends);
            failuresMap["invalid"] = OSD.FromInteger(FailuresInvalid);
            failuresMap["off_circuit"] = OSD.FromInteger(FailuresOffCircuit);
            failuresMap["resent"] = OSD.FromInteger(FailuresResent);
            failuresMap["send_packet"] = OSD.FromInteger(FailuresSendPacket);
            statsMap["failures"] = failuresMap;

            OSDMap statsMiscMap = new OSDMap(3);
            statsMiscMap["int_1"] = OSD.FromInteger(MiscInt1);
            statsMiscMap["int_2"] = OSD.FromInteger(MiscInt2);
            statsMiscMap["string_1"] = OSD.FromString(MiscString1);
            statsMap["misc"] = statsMiscMap;

            OSDMap netMap = new OSDMap(3);

            // in
            OSDMap netInMap = new OSDMap(4);
            netInMap["compressed_packets"] = OSD.FromInteger(InCompressedPackets);
            netInMap["kbytes"] = OSD.FromReal(InKbytes);
            netInMap["packets"] = OSD.FromReal(InPackets);
            netInMap["savings"] = OSD.FromReal(InSavings);
            netMap["in"] = netInMap;
            // out
            OSDMap netOutMap = new OSDMap(4);
            netOutMap["compressed_packets"] = OSD.FromInteger(OutCompressedPackets);
            netOutMap["kbytes"] = OSD.FromReal(OutKbytes);
            netOutMap["packets"] = OSD.FromReal(OutPackets);
            netOutMap["savings"] = OSD.FromReal(OutSavings);
            netMap["out"] = netOutMap;

            statsMap["net"] = netMap;

            //system
            OSDMap systemStatsMap = new OSDMap(7);
            systemStatsMap["cpu"] = OSD.FromString(SystemCPU);
            systemStatsMap["gpu"] = OSD.FromString(SystemGPU);
            systemStatsMap["gpu_class"] = OSD.FromInteger(SystemGPUClass);
            systemStatsMap["gpu_vendor"] = OSD.FromString(SystemGPUVendor);
            systemStatsMap["gpu_version"] = OSD.FromString(SystemGPUVersion);
            systemStatsMap["os"] = OSD.FromString(SystemOS);
            systemStatsMap["ram"] = OSD.FromInteger(SystemInstalledRam);
            map["system"] = systemStatsMap;

            map["stats"] = statsMap;
            return map;
        }

        public void Deserialize(OSDMap map)
        {
            SessionID = map["session_id"].AsUUID();

            OSDMap agentMap = (OSDMap)map["agent"];
            AgentsInView = agentMap["agents_in_view"].AsInteger();
            AgentFPS = (float)agentMap["fps"].AsReal();
            AgentLanguage = agentMap["language"].AsString();
            AgentMemoryUsed = (float)agentMap["mem_use"].AsReal();
            MetersTraveled = agentMap["meters_traveled"].AsInteger();
            AgentPing = (float)agentMap["ping"].AsReal();
            RegionsVisited = agentMap["regions_visited"].AsInteger();
            AgentRuntime = (float)agentMap["run_time"].AsReal();
            SimulatorFPS = (float)agentMap["sim_fps"].AsReal();
            AgentStartTime = Utils.UnixTimeToDateTime(agentMap["start_time"].AsUInteger());
            AgentVersion = agentMap["version"].AsString();

            OSDMap downloadsMap = (OSDMap)map["downloads"];
            object_kbytes = (float)downloadsMap["object_kbytes"].AsReal();
            texture_kbytes = (float)downloadsMap["texture_kbytes"].AsReal();
            world_kbytes = (float)downloadsMap["world_kbytes"].AsReal();

            OSDMap miscMap = (OSDMap)map["misc"];
            MiscVersion = (float)miscMap["Version"].AsReal();
            VertexBuffersEnabled = miscMap["Vertex Buffers Enabled"].AsBoolean();

            OSDMap statsMap = (OSDMap)map["stats"];
            OSDMap failuresMap = (OSDMap)statsMap["failures"];
            StatsDropped = failuresMap["dropped"].AsInteger();
            StatsFailedResends = failuresMap["failed_resends"].AsInteger();
            FailuresInvalid = failuresMap["invalid"].AsInteger();
            FailuresOffCircuit = failuresMap["off_circuit"].AsInteger();
            FailuresResent = failuresMap["resent"].AsInteger();
            FailuresSendPacket = failuresMap["send_packet"].AsInteger();

            OSDMap statsMiscMap = (OSDMap)statsMap["misc"];
            MiscInt1 = statsMiscMap["int_1"].AsInteger();
            MiscInt2 = statsMiscMap["int_2"].AsInteger();
            MiscString1 = statsMiscMap["string_1"].AsString();
            OSDMap netMap = (OSDMap)statsMap["net"];
            // in
            OSDMap netInMap = (OSDMap)netMap["in"];
            InCompressedPackets = netInMap["compressed_packets"].AsInteger();
            InKbytes = netInMap["kbytes"].AsInteger();
            InPackets = netInMap["packets"].AsInteger();
            InSavings = netInMap["savings"].AsInteger();
            // out
            OSDMap netOutMap = (OSDMap)netMap["out"];
            OutCompressedPackets = netOutMap["compressed_packets"].AsInteger();
            OutKbytes = netOutMap["kbytes"].AsInteger();
            OutPackets = netOutMap["packets"].AsInteger();
            OutSavings = netOutMap["savings"].AsInteger();

            //system
            OSDMap systemStatsMap = (OSDMap)map["system"];
            SystemCPU = systemStatsMap["cpu"].AsString();
            SystemGPU = systemStatsMap["gpu"].AsString();
            SystemGPUClass = systemStatsMap["gpu_class"].AsInteger();
            SystemGPUVendor = systemStatsMap["gpu_vendor"].AsString();
            SystemGPUVersion = systemStatsMap["gpu_version"].AsString();
            SystemOS = systemStatsMap["os"].AsString();
            SystemInstalledRam = systemStatsMap["ram"].AsInteger();
        }
    }

    #endregion
}