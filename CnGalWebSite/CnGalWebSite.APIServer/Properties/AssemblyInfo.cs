using System.Runtime.CompilerServices;

// 让单元测试直接校验审核写入边界的契约（AllowedKeys/IsGuarded），
// 而不必通过反射或完整业务服务构造。
[assembly: InternalsVisibleTo("CnGalWebSite.UnitTest")]
