using System;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Web;
using System.Web.Script.Services;
using System.Web.Services;
using System.Xml;

namespace AirWebService
{
    /// <summary>
    /// Queue 웹서비스
    /// </summary>
    [WebService(Namespace = "http://airservice2.modetour.com/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [ToolboxItem(false)]
    [ScriptService]
    public class QueueService : System.Web.Services.WebService
    {
        Common cm;
        ModeConfig mc;
        AmadeusAirService amd;
        LogSave log;
        HttpContext hcc;

        public QueueService()
        {
            cm = new Common();
            mc = new ModeConfig();
            amd = new AmadeusAirService();
            log = new LogSave();
            hcc = HttpContext.Current;
        }

        #region "GUID"

        /// <summary>
        /// 고유번호 생성
        /// </summary>
        /// <returns></returns>
        [WebMethod(Description = "고유번호 생성")]
        public string GUID()
        {
            return cm.GetGUID;
        }

        #endregion "GUID"

        #region "세션"

        /// <summary>
        /// 세션생성
        /// </summary>
        /// <param name="SNM">사이트번호</param>
        /// <param name="GUID">고유번호</param>
        /// <returns></returns>
        [WebMethod(Description = "세션생성")]
        public XmlElement SessionCreate(int SNM, string GUID)
        {
            string LogGUID = cm.GetGUID;
            
            //파라미터 로그 기록
            try
            {
                SqlParameter[] sqlParam = new SqlParameter[] {
                        new SqlParameter("@서비스번호", SqlDbType.Int, 0),
                        new SqlParameter("@사이트번호", SqlDbType.Int, 0),
                        new SqlParameter("@요청단말기", SqlDbType.VarChar, 30),
                        new SqlParameter("@서버명", SqlDbType.VarChar, 20),
                        new SqlParameter("@메서드", SqlDbType.VarChar, 10),
                        new SqlParameter("@사용자IP", SqlDbType.VarChar, 30),
                        new SqlParameter("@GUID", SqlDbType.VarChar, 50),
                        new SqlParameter("@요청1", SqlDbType.VarChar, 3000)
                    };

                sqlParam[0].Value = 410;
                sqlParam[1].Value = SNM;
                sqlParam[2].Value = "";
                sqlParam[3].Value = Environment.MachineName;
                sqlParam[4].Value = hcc.Request.HttpMethod;
                sqlParam[5].Value = hcc.Request.UserHostAddress;
                sqlParam[6].Value = LogGUID;
                sqlParam[7].Value = GUID;

                log.LogDBSave(sqlParam);
            }
            finally { }
            
            return amd.Authenticate(SNM, String.Concat(GUID, "-01"));
        }

        /// <summary>
        /// 세션종료
        /// </summary>
        /// <param name="SID">SessionId</param>
        /// <param name="SQN">SequenceNumber</param>
        /// <param name="SCT">SecurityToken</param>
        /// <param name="GUID">고유번호</param>
        /// <returns>0:성공, 그 외에는 실패</returns>
        [WebMethod(Description = "세션종료")]
        public int SessionClose(string SID, string SQN, string SCT, string GUID)
        {
            string LogGUID = cm.GetGUID;

            //파라미터 로그 기록
            try
            {
                SqlParameter[] sqlParam = new SqlParameter[] {
                        new SqlParameter("@서비스번호", SqlDbType.Int, 0),
                        new SqlParameter("@사이트번호", SqlDbType.Int, 0),
                        new SqlParameter("@요청단말기", SqlDbType.VarChar, 30),
                        new SqlParameter("@서버명", SqlDbType.VarChar, 20),
                        new SqlParameter("@메서드", SqlDbType.VarChar, 10),
                        new SqlParameter("@사용자IP", SqlDbType.VarChar, 30),
                        new SqlParameter("@GUID", SqlDbType.VarChar, 50),
                        new SqlParameter("@요청1", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청2", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청3", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청4", SqlDbType.VarChar, 3000)
                    };

                sqlParam[0].Value = 409;
                sqlParam[1].Value = 0;
                sqlParam[2].Value = "";
                sqlParam[3].Value = Environment.MachineName;
                sqlParam[4].Value = hcc.Request.HttpMethod;
                sqlParam[5].Value = hcc.Request.UserHostAddress;
                sqlParam[6].Value = LogGUID;
                sqlParam[7].Value = SID;
                sqlParam[8].Value = SQN;
                sqlParam[9].Value = SCT;
                sqlParam[10].Value = GUID;

                log.LogDBSave(sqlParam);
            }
            finally { }
            
            return amd.SignOut(SID, SQN, SCT, String.Concat(GUID, "-", cm.NumPosition(SQN, 2)));
        }

        #endregion "세션"

        /// <summary>
        /// 큐방 또는 카테고리별 PNR 카운트
        /// </summary>
        /// <param name="SID">SessionId</param>
        /// <param name="SQN">SequenceNumber</param>
        /// <param name="SCT">SecurityToken</param>
        /// <param name="GUID">고유번호</param>
        /// <param name="OFID">OfficeId</param>
        /// <param name="QueueNumber">큐방번호</param>
        /// <param name="CategoryNumber">카테고리번호</param>
        /// <returns></returns>
        [WebMethod(Description = "큐방 또는 카테고리별 PNR 카운트")]
        public XmlElement QueueCountTotalRS(string SID, string SQN, string SCT, string GUID, string OFID, string QueueNumber, string CategoryNumber)
        {
            string LogGUID = cm.GetGUID;

            //파라미터 로그 기록
            try
            {
                SqlParameter[] sqlParam = new SqlParameter[] {
                        new SqlParameter("@서비스번호", SqlDbType.Int, 0),
                        new SqlParameter("@사이트번호", SqlDbType.Int, 0),
                        new SqlParameter("@요청단말기", SqlDbType.VarChar, 30),
                        new SqlParameter("@서버명", SqlDbType.VarChar, 20),
                        new SqlParameter("@메서드", SqlDbType.VarChar, 10),
                        new SqlParameter("@사용자IP", SqlDbType.VarChar, 30),
                        new SqlParameter("@GUID", SqlDbType.VarChar, 50),
                        new SqlParameter("@요청1", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청2", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청3", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청4", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청5", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청6", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청7", SqlDbType.VarChar, 3000)
                    };

                sqlParam[0].Value = 404;
                sqlParam[1].Value = 0;
                sqlParam[2].Value = "";
                sqlParam[3].Value = Environment.MachineName;
                sqlParam[4].Value = hcc.Request.HttpMethod;
                sqlParam[5].Value = hcc.Request.UserHostAddress;
                sqlParam[6].Value = LogGUID;
                sqlParam[7].Value = SID;
                sqlParam[8].Value = SQN;
                sqlParam[9].Value = SCT;
                sqlParam[10].Value = GUID;
                sqlParam[11].Value = OFID;
                sqlParam[12].Value = QueueNumber;
                sqlParam[13].Value = CategoryNumber;

                log.LogDBSave(sqlParam);
            }
            finally { }
            
            try
            {
                return amd.QueueCountTotalRS(SID, SQN, SCT, String.Concat(GUID, "-", cm.NumPosition(SQN, 2)), OFID, (cm.IsInteger(QueueNumber) ? cm.RequestInt(QueueNumber) : (int?)null), (cm.IsInteger(CategoryNumber) ? cm.RequestInt(CategoryNumber) : (int?)null));
            }
            catch (Exception ex)
            {
                return new MWSExceptionMode(ex, hcc, LogGUID, "QueueService", MethodBase.GetCurrentMethod().Name, 404, 0, 0).ToErrors;
            }
        }

        /// <summary>
        /// 특정 큐방/카테고리 내의 PNR 리스트를 조회
        /// </summary>
        /// <param name="SID">SessionId</param>
        /// <param name="SQN">SequenceNumber</param>
        /// <param name="SCT">SecurityToken</param>
        /// <param name="GUID">고유번호</param>
        /// <param name="OFID">OfficeId</param>
        /// <param name="QueueNumber">큐방번호</param>
        /// <param name="CategoryType">카테고리구분(C:카테고리, 1~4:Date range)</param>
        /// <param name="CategoryNumber">카테고리번호</param>
        /// <param name="CategoryName">카테고리명</param>
        /// <param name="RangeSNumber">조회범위 시작번호</param>
        /// <param name="RangeENumber">조회범위 마침번호</param>
        /// <returns></returns>
        [WebMethod(Description = "특정 큐방/카테고리 내의 PNR 리스트를 조회")]
        public XmlElement QueueListRS(string SID, string SQN, string SCT, string GUID, string OFID, int QueueNumber, string CategoryType, int CategoryNumber, string CategoryName, int RangeSNumber, int RangeENumber)
        {
            string LogGUID = cm.GetGUID;

            //파라미터 로그 기록
            try
            {
                SqlParameter[] sqlParam = new SqlParameter[] {
                        new SqlParameter("@서비스번호", SqlDbType.Int, 0),
                        new SqlParameter("@사이트번호", SqlDbType.Int, 0),
                        new SqlParameter("@요청단말기", SqlDbType.VarChar, 30),
                        new SqlParameter("@서버명", SqlDbType.VarChar, 20),
                        new SqlParameter("@메서드", SqlDbType.VarChar, 10),
                        new SqlParameter("@사용자IP", SqlDbType.VarChar, 30),
                        new SqlParameter("@GUID", SqlDbType.VarChar, 50),
                        new SqlParameter("@요청1", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청2", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청3", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청4", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청5", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청6", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청7", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청8", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청9", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청10", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청11", SqlDbType.VarChar, 3000)
                    };

                sqlParam[0].Value = 405;
                sqlParam[1].Value = 0;
                sqlParam[2].Value = "";
                sqlParam[3].Value = Environment.MachineName;
                sqlParam[4].Value = hcc.Request.HttpMethod;
                sqlParam[5].Value = hcc.Request.UserHostAddress;
                sqlParam[6].Value = LogGUID;
                sqlParam[7].Value = SID;
                sqlParam[8].Value = SQN;
                sqlParam[9].Value = SCT;
                sqlParam[10].Value = GUID;
                sqlParam[11].Value = OFID;
                sqlParam[12].Value = QueueNumber;
                sqlParam[13].Value = CategoryType;
                sqlParam[14].Value = CategoryNumber;
                sqlParam[15].Value = CategoryName;
                sqlParam[16].Value = RangeSNumber;
                sqlParam[17].Value = RangeENumber;

                log.LogDBSave(sqlParam);
            }
            finally { }
            
            try
            {
                return amd.QueueListRS(SID, SQN, SCT, String.Concat(GUID, "-", cm.NumPosition(SQN, 2)), OFID, QueueNumber, CategoryType, CategoryNumber, CategoryName, RangeSNumber, RangeENumber);
            }
            catch (Exception ex)
            {
                return new MWSExceptionMode(ex, hcc, LogGUID, "QueueService", MethodBase.GetCurrentMethod().Name, 405, 0, 0).ToErrors;
            }
        }

        /// <summary>
        /// 특정 큐방/카테고리에 PNR 배치
        /// </summary>
        /// <param name="SID">SessionId</param>
        /// <param name="SQN">SequenceNumber</param>
        /// <param name="SCT">SecurityToken</param>
        /// <param name="GUID">고유번호</param>
        /// <param name="OFID">OfficeId</param>
        /// <param name="QueueNumber">큐방번호</param>
        /// <param name="CategoryType">카테고리구분(C:카테고리, 1~4:Date range)</param>
        /// <param name="CategoryNumber">카테고리번호</param>
        /// <param name="CategoryName">카테고리명</param>
        /// <returns></returns>
        [WebMethod(Description = "특정 큐방/카테고리에 PNR 배치")]
        public XmlElement QueuePlacePNRRS(string SID, string SQN, string SCT, string GUID, string OFID, int QueueNumber, string CategoryType, int CategoryNumber, string CategoryName, string PNR)
        {
            string LogGUID = cm.GetGUID;

            //파라미터 로그 기록
            try
            {
                SqlParameter[] sqlParam = new SqlParameter[] {
                        new SqlParameter("@서비스번호", SqlDbType.Int, 0),
                        new SqlParameter("@사이트번호", SqlDbType.Int, 0),
                        new SqlParameter("@요청단말기", SqlDbType.VarChar, 30),
                        new SqlParameter("@서버명", SqlDbType.VarChar, 20),
                        new SqlParameter("@메서드", SqlDbType.VarChar, 10),
                        new SqlParameter("@사용자IP", SqlDbType.VarChar, 30),
                        new SqlParameter("@GUID", SqlDbType.VarChar, 50),
                        new SqlParameter("@요청1", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청2", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청3", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청4", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청5", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청6", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청7", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청8", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청9", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청10", SqlDbType.VarChar, 3000)
                    };

                sqlParam[0].Value = 407;
                sqlParam[1].Value = 0;
                sqlParam[2].Value = "";
                sqlParam[3].Value = Environment.MachineName;
                sqlParam[4].Value = hcc.Request.HttpMethod;
                sqlParam[5].Value = hcc.Request.UserHostAddress;
                sqlParam[6].Value = LogGUID;
                sqlParam[7].Value = SID;
                sqlParam[8].Value = SQN;
                sqlParam[9].Value = SCT;
                sqlParam[10].Value = GUID;
                sqlParam[11].Value = OFID;
                sqlParam[12].Value = QueueNumber;
                sqlParam[13].Value = CategoryType;
                sqlParam[14].Value = CategoryNumber;
                sqlParam[15].Value = CategoryName;
                sqlParam[16].Value = PNR;

                log.LogDBSave(sqlParam);
            }
            finally { }
            
            try
            {
                return amd.QueuePlacePNRRS(SID, SQN, SCT, String.Concat(GUID, "-", cm.NumPosition(SQN, 2)), OFID, QueueNumber, CategoryType, CategoryNumber, CategoryName, PNR);
            }
            catch (Exception ex)
            {
                return new MWSExceptionMode(ex, hcc, LogGUID, "QueueService", MethodBase.GetCurrentMethod().Name, 407, 0, 0).ToErrors;
            }
        }

        /// <summary>
        /// 특정 큐방/카테고리 내의 PNR 리스트를 복사
        /// </summary>
        /// <param name="SID">SessionId</param>
        /// <param name="SQN">SequenceNumber</param>
        /// <param name="SCT">SecurityToken</param>
        /// <param name="GUID">고유번호</param>
        /// <param name="OFIDFrom">OfficeId(복사될 원본)</param>
        /// <param name="QueueNumberFrom">큐방번호(복사될 원본)</param>
        /// <param name="CategoryTypeFrom">카테고리구분(C:카테고리, 1~4:Date range)</param>
        /// <param name="CategoryNumberFrom">카테고리번호(복사될 원본)</param>
        /// <param name="CategoryNameFrom">카테고리명</param>
        /// <param name="OFIDTo">OfficeId(복사된 정보)</param>
        /// <param name="QueueNumberTo">큐방번호(복사된 정보)</param>
        /// <param name="CategoryTypeTo">카테고리구분(C:카테고리, 1~4:Date range)</param>
        /// <param name="CategoryNumberTo">카테고리번호(복사된 정보)</param>
        /// <param name="CategoryNameTo">카테고리명</param>
        /// <returns></returns>
        [WebMethod(Description = "특정 큐방/카테고리 내의 PNR 리스트를 복사")]
        public XmlElement QueueMoveItemRS(string SID, string SQN, string SCT, string GUID, string OFIDFrom, int QueueNumberFrom, string CategoryTypeFrom, int CategoryNumberFrom, string CategoryNameFrom, string OFIDTo, int QueueNumberTo, string CategoryTypeTo, int CategoryNumberTo, string CategoryNameTo)
        {
            string LogGUID = cm.GetGUID;

            //파라미터 로그 기록
            try
            {
                SqlParameter[] sqlParam = new SqlParameter[] {
                        new SqlParameter("@서비스번호", SqlDbType.Int, 0),
                        new SqlParameter("@사이트번호", SqlDbType.Int, 0),
                        new SqlParameter("@요청단말기", SqlDbType.VarChar, 30),
                        new SqlParameter("@서버명", SqlDbType.VarChar, 20),
                        new SqlParameter("@메서드", SqlDbType.VarChar, 10),
                        new SqlParameter("@사용자IP", SqlDbType.VarChar, 30),
                        new SqlParameter("@GUID", SqlDbType.VarChar, 50),
                        new SqlParameter("@요청1", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청2", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청3", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청4", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청5", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청6", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청7", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청8", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청9", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청10", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청11", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청12", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청13", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청14", SqlDbType.VarChar, 3000)
                    };

                sqlParam[0].Value = 406;
                sqlParam[1].Value = 0;
                sqlParam[2].Value = "";
                sqlParam[3].Value = Environment.MachineName;
                sqlParam[4].Value = hcc.Request.HttpMethod;
                sqlParam[5].Value = hcc.Request.UserHostAddress;
                sqlParam[6].Value = LogGUID;
                sqlParam[7].Value = SID;
                sqlParam[8].Value = SQN;
                sqlParam[9].Value = SCT;
                sqlParam[10].Value = GUID;
                sqlParam[11].Value = OFIDFrom;
                sqlParam[12].Value = QueueNumberFrom;
                sqlParam[13].Value = CategoryTypeFrom;
                sqlParam[14].Value = CategoryNumberFrom;
                sqlParam[15].Value = CategoryNameFrom;
                sqlParam[16].Value = OFIDTo;
                sqlParam[17].Value = QueueNumberTo;
                sqlParam[18].Value = CategoryTypeTo;
                sqlParam[19].Value = CategoryNumberTo;
                sqlParam[20].Value = CategoryNameTo;

                log.LogDBSave(sqlParam);
            }
            finally { }
            
            try
            {
                return amd.QueueMoveItemRS(SID, SQN, SCT, String.Concat(GUID, "-", cm.NumPosition(SQN, 2)), OFIDFrom, QueueNumberFrom, CategoryTypeFrom, CategoryNumberFrom, CategoryNameFrom, OFIDTo, QueueNumberTo, CategoryTypeTo, CategoryNumberTo, CategoryNameTo);
            }
            catch (Exception ex)
            {
                return new MWSExceptionMode(ex, hcc, LogGUID, "QueueService", MethodBase.GetCurrentMethod().Name, 406, 0, 0).ToErrors;
            }
        }

        /// <summary>
        /// 특정 큐방/카테고리 내의 PNR 삭제
        /// </summary>
        /// <param name="SID">SessionId</param>
        /// <param name="SQN">SequenceNumber</param>
        /// <param name="SCT">SecurityToken</param>
        /// <param name="GUID">고유번호</param>
        /// <param name="OFID">OfficeId</param>
        /// <param name="QueueNumber">큐방번호</param>
        /// <param name="CategoryType">카테고리구분(C:카테고리, 1~4:Date range)</param>
        /// <param name="CategoryNumber">카테고리번호</param>
        /// <param name="CategoryName">카테고리명</param>
        /// <param name="PNR">PNR번호</param>
        /// <returns></returns>
        [WebMethod(Description = "특정 큐방/카테고리 내의 PNR 삭제")]
        public XmlElement QueueRemoveItemRS(string SID, string SQN, string SCT, string GUID, string OFID, int QueueNumber, string CategoryType, int CategoryNumber, string CategoryName, string PNR)
        {
            string LogGUID = cm.GetGUID;

            //파라미터 로그 기록
            try
            {
                SqlParameter[] sqlParam = new SqlParameter[] {
                        new SqlParameter("@서비스번호", SqlDbType.Int, 0),
                        new SqlParameter("@사이트번호", SqlDbType.Int, 0),
                        new SqlParameter("@요청단말기", SqlDbType.VarChar, 30),
                        new SqlParameter("@서버명", SqlDbType.VarChar, 20),
                        new SqlParameter("@메서드", SqlDbType.VarChar, 10),
                        new SqlParameter("@사용자IP", SqlDbType.VarChar, 30),
                        new SqlParameter("@GUID", SqlDbType.VarChar, 50),
                        new SqlParameter("@요청1", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청2", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청3", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청4", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청5", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청6", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청7", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청8", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청9", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청10", SqlDbType.VarChar, 3000)
                    };

                sqlParam[0].Value = 408;
                sqlParam[1].Value = 0;
                sqlParam[2].Value = "";
                sqlParam[3].Value = Environment.MachineName;
                sqlParam[4].Value = hcc.Request.HttpMethod;
                sqlParam[5].Value = hcc.Request.UserHostAddress;
                sqlParam[6].Value = LogGUID;
                sqlParam[7].Value = SID;
                sqlParam[8].Value = SQN;
                sqlParam[9].Value = SCT;
                sqlParam[10].Value = GUID;
                sqlParam[11].Value = OFID;
                sqlParam[12].Value = QueueNumber;
                sqlParam[13].Value = CategoryType;
                sqlParam[14].Value = CategoryNumber;
                sqlParam[15].Value = CategoryName;
                sqlParam[16].Value = PNR;

                log.LogDBSave(sqlParam);
            }
            finally { }
            
            try
            {
                return amd.QueueRemoveItemRS(SID, SQN, SCT, String.Concat(GUID, "-", cm.NumPosition(SQN, 2)), OFID, QueueNumber, CategoryType, CategoryNumber, CategoryName, PNR);
            }
            catch (Exception ex)
            {
                return new MWSExceptionMode(ex, hcc, LogGUID, "QueueService", MethodBase.GetCurrentMethod().Name, 408, 0, 0).ToErrors;
            }
        }

        /// <summary>
        /// PNR 조회(Command)
        /// </summary>
        /// <param name="SID">SessionId</param>
        /// <param name="SQN">SequenceNumber</param>
        /// <param name="SCT">SecurityToken</param>
        /// <param name="GUID">고유번호</param>
        /// <param name="PNR">PNR번호</param>
        /// <returns></returns>
        [WebMethod(Description = "PNR 조회(Command)")]
        public XmlElement PNRRetrieveByCommand(string SID, string SQN, string SCT, string GUID, string PNR)
        {
            string LogGUID = cm.GetGUID;

            //파라미터 로그 기록
            try
            {
                SqlParameter[] sqlParam = new SqlParameter[] {
                        new SqlParameter("@서비스번호", SqlDbType.Int, 0),
                        new SqlParameter("@사이트번호", SqlDbType.Int, 0),
                        new SqlParameter("@요청단말기", SqlDbType.VarChar, 30),
                        new SqlParameter("@서버명", SqlDbType.VarChar, 20),
                        new SqlParameter("@메서드", SqlDbType.VarChar, 10),
                        new SqlParameter("@사용자IP", SqlDbType.VarChar, 30),
                        new SqlParameter("@GUID", SqlDbType.VarChar, 50),
                        new SqlParameter("@요청1", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청2", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청3", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청4", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청5", SqlDbType.VarChar, 3000)
                    };

                sqlParam[0].Value = 402;
                sqlParam[1].Value = 0;
                sqlParam[2].Value = "";
                sqlParam[3].Value = Environment.MachineName;
                sqlParam[4].Value = hcc.Request.HttpMethod;
                sqlParam[5].Value = hcc.Request.UserHostAddress;
                sqlParam[6].Value = LogGUID;
                sqlParam[7].Value = SID;
                sqlParam[8].Value = SQN;
                sqlParam[9].Value = SCT;
                sqlParam[10].Value = GUID;
                sqlParam[11].Value = PNR;

                log.LogDBSave(sqlParam);
            }
            finally { }
            
            try
            {
                return amd.CommandCrypticRS(SID, SQN, SCT, GUID, String.Concat("RT", PNR));
            }
            catch (Exception ex)
            {
                return new MWSExceptionMode(ex, hcc, LogGUID, "QueueService", MethodBase.GetCurrentMethod().Name, 402, 0, 0).ToErrors;
            }
        }

        /// <summary>
        /// PNR 조회(Command)(RTW)
        /// </summary>
        /// <param name="SNM">사이트번호</param>
        /// <param name="GDS">GDS명</param>
        /// <param name="PNR">PNR번호</param>
        /// <returns></returns>
        [WebMethod(Description = "PNR 조회(Command)(RTW)")]
        public XmlElement PNRRetrieveByCommandRTW(int SNM, string GDS, string PNR)
        {
            string LogGUID = cm.GetGUID;

            //파라미터 로그 기록
            try
            {
                SqlParameter[] sqlParam = new SqlParameter[] {
                        new SqlParameter("@서비스번호", SqlDbType.Int, 0),
                        new SqlParameter("@사이트번호", SqlDbType.Int, 0),
                        new SqlParameter("@요청단말기", SqlDbType.VarChar, 30),
                        new SqlParameter("@서버명", SqlDbType.VarChar, 20),
                        new SqlParameter("@메서드", SqlDbType.VarChar, 10),
                        new SqlParameter("@사용자IP", SqlDbType.VarChar, 30),
                        new SqlParameter("@GUID", SqlDbType.VarChar, 50),
                        new SqlParameter("@요청1", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청2", SqlDbType.VarChar, 3000)
                    };

                sqlParam[0].Value = 403;
                sqlParam[1].Value = SNM;
                sqlParam[2].Value = "";
                sqlParam[3].Value = Environment.MachineName;
                sqlParam[4].Value = hcc.Request.HttpMethod;
                sqlParam[5].Value = hcc.Request.UserHostAddress;
                sqlParam[6].Value = LogGUID;
                sqlParam[7].Value = GDS;
                sqlParam[8].Value = PNR;

                log.LogDBSave(sqlParam);
            }
            finally { }
            
            return new ModeService().PNRRetrieveRTW(SNM, GDS, PNR);
        }

        /// <summary>
        /// PNR 조회(API)
        /// </summary>
        /// <param name="SID">SessionId</param>
        /// <param name="SQN">SequenceNumber</param>
        /// <param name="SCT">SecurityToken</param>
        /// <param name="GUID">고유번호</param>
        /// <param name="PNR">PNR번호</param>
        /// <returns></returns>
        [WebMethod(Description = "PNR 조회(API)")]
        public XmlElement PNRRetrieve(string SID, string SQN, string SCT, string GUID, string PNR)
        {
            string LogGUID = cm.GetGUID;

            //파라미터 로그 기록
            try
            {
                SqlParameter[] sqlParam = new SqlParameter[] {
                        new SqlParameter("@서비스번호", SqlDbType.Int, 0),
                        new SqlParameter("@사이트번호", SqlDbType.Int, 0),
                        new SqlParameter("@요청단말기", SqlDbType.VarChar, 30),
                        new SqlParameter("@서버명", SqlDbType.VarChar, 20),
                        new SqlParameter("@메서드", SqlDbType.VarChar, 10),
                        new SqlParameter("@사용자IP", SqlDbType.VarChar, 30),
                        new SqlParameter("@GUID", SqlDbType.VarChar, 50),
                        new SqlParameter("@요청1", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청2", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청3", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청4", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청5", SqlDbType.VarChar, 3000)
                    };

                sqlParam[0].Value = 401;
                sqlParam[1].Value = 0;
                sqlParam[2].Value = "";
                sqlParam[3].Value = Environment.MachineName;
                sqlParam[4].Value = hcc.Request.HttpMethod;
                sqlParam[5].Value = hcc.Request.UserHostAddress;
                sqlParam[6].Value = LogGUID;
                sqlParam[7].Value = SID;
                sqlParam[8].Value = SQN;
                sqlParam[9].Value = SCT;
                sqlParam[10].Value = GUID;
                sqlParam[11].Value = PNR;

                log.LogDBSave(sqlParam);
            }
            finally { }
            
            try
            {
                return amd.RetrieveRS(SID, SQN, SCT, GUID, PNR);
            }
            catch (Exception ex)
            {
                return new MWSExceptionMode(ex, hcc, LogGUID, "QueueService", MethodBase.GetCurrentMethod().Name, 401, 0, 0).ToErrors;
            }
        }
    }
}