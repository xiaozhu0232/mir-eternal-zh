using System;
using System.Text;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Math.EC;

internal class LongArray
{
	private const string ZEROES = "0000000000000000000000000000000000000000000000000000000000000000";

	private static readonly ushort[] INTERLEAVE2_TABLE = new ushort[256]
	{
		0, 1, 4, 5, 16, 17, 20, 21, 64, 65,
		68, 69, 80, 81, 84, 85, 256, 257, 260, 261,
		272, 273, 276, 277, 320, 321, 324, 325, 336, 337,
		340, 341, 1024, 1025, 1028, 1029, 1040, 1041, 1044, 1045,
		1088, 1089, 1092, 1093, 1104, 1105, 1108, 1109, 1280, 1281,
		1284, 1285, 1296, 1297, 1300, 1301, 1344, 1345, 1348, 1349,
		1360, 1361, 1364, 1365, 4096, 4097, 4100, 4101, 4112, 4113,
		4116, 4117, 4160, 4161, 4164, 4165, 4176, 4177, 4180, 4181,
		4352, 4353, 4356, 4357, 4368, 4369, 4372, 4373, 4416, 4417,
		4420, 4421, 4432, 4433, 4436, 4437, 5120, 5121, 5124, 5125,
		5136, 5137, 5140, 5141, 5184, 5185, 5188, 5189, 5200, 5201,
		5204, 5205, 5376, 5377, 5380, 5381, 5392, 5393, 5396, 5397,
		5440, 5441, 5444, 5445, 5456, 5457, 5460, 5461, 16384, 16385,
		16388, 16389, 16400, 16401, 16404, 16405, 16448, 16449, 16452, 16453,
		16464, 16465, 16468, 16469, 16640, 16641, 16644, 16645, 16656, 16657,
		16660, 16661, 16704, 16705, 16708, 16709, 16720, 16721, 16724, 16725,
		17408, 17409, 17412, 17413, 17424, 17425, 17428, 17429, 17472, 17473,
		17476, 17477, 17488, 17489, 17492, 17493, 17664, 17665, 17668, 17669,
		17680, 17681, 17684, 17685, 17728, 17729, 17732, 17733, 17744, 17745,
		17748, 17749, 20480, 20481, 20484, 20485, 20496, 20497, 20500, 20501,
		20544, 20545, 20548, 20549, 20560, 20561, 20564, 20565, 20736, 20737,
		20740, 20741, 20752, 20753, 20756, 20757, 20800, 20801, 20804, 20805,
		20816, 20817, 20820, 20821, 21504, 21505, 21508, 21509, 21520, 21521,
		21524, 21525, 21568, 21569, 21572, 21573, 21584, 21585, 21588, 21589,
		21760, 21761, 21764, 21765, 21776, 21777, 21780, 21781, 21824, 21825,
		21828, 21829, 21840, 21841, 21844, 21845
	};

	private static readonly int[] INTERLEAVE3_TABLE = new int[128]
	{
		0, 1, 8, 9, 64, 65, 72, 73, 512, 513,
		520, 521, 576, 577, 584, 585, 4096, 4097, 4104, 4105,
		4160, 4161, 4168, 4169, 4608, 4609, 4616, 4617, 4672, 4673,
		4680, 4681, 32768, 32769, 32776, 32777, 32832, 32833, 32840, 32841,
		33280, 33281, 33288, 33289, 33344, 33345, 33352, 33353, 36864, 36865,
		36872, 36873, 36928, 36929, 36936, 36937, 37376, 37377, 37384, 37385,
		37440, 37441, 37448, 37449, 262144, 262145, 262152, 262153, 262208, 262209,
		262216, 262217, 262656, 262657, 262664, 262665, 262720, 262721, 262728, 262729,
		266240, 266241, 266248, 266249, 266304, 266305, 266312, 266313, 266752, 266753,
		266760, 266761, 266816, 266817, 266824, 266825, 294912, 294913, 294920, 294921,
		294976, 294977, 294984, 294985, 295424, 295425, 295432, 295433, 295488, 295489,
		295496, 295497, 299008, 299009, 299016, 299017, 299072, 299073, 299080, 299081,
		299520, 299521, 299528, 299529, 299584, 299585, 299592, 299593
	};

	private static readonly int[] INTERLEAVE4_TABLE = new int[256]
	{
		0, 1, 16, 17, 256, 257, 272, 273, 4096, 4097,
		4112, 4113, 4352, 4353, 4368, 4369, 65536, 65537, 65552, 65553,
		65792, 65793, 65808, 65809, 69632, 69633, 69648, 69649, 69888, 69889,
		69904, 69905, 1048576, 1048577, 1048592, 1048593, 1048832, 1048833, 1048848, 1048849,
		1052672, 1052673, 1052688, 1052689, 1052928, 1052929, 1052944, 1052945, 1114112, 1114113,
		1114128, 1114129, 1114368, 1114369, 1114384, 1114385, 1118208, 1118209, 1118224, 1118225,
		1118464, 1118465, 1118480, 1118481, 16777216, 16777217, 16777232, 16777233, 16777472, 16777473,
		16777488, 16777489, 16781312, 16781313, 16781328, 16781329, 16781568, 16781569, 16781584, 16781585,
		16842752, 16842753, 16842768, 16842769, 16843008, 16843009, 16843024, 16843025, 16846848, 16846849,
		16846864, 16846865, 16847104, 16847105, 16847120, 16847121, 17825792, 17825793, 17825808, 17825809,
		17826048, 17826049, 17826064, 17826065, 17829888, 17829889, 17829904, 17829905, 17830144, 17830145,
		17830160, 17830161, 17891328, 17891329, 17891344, 17891345, 17891584, 17891585, 17891600, 17891601,
		17895424, 17895425, 17895440, 17895441, 17895680, 17895681, 17895696, 17895697, 268435456, 268435457,
		268435472, 268435473, 268435712, 268435713, 268435728, 268435729, 268439552, 268439553, 268439568, 268439569,
		268439808, 268439809, 268439824, 268439825, 268500992, 268500993, 268501008, 268501009, 268501248, 268501249,
		268501264, 268501265, 268505088, 268505089, 268505104, 268505105, 268505344, 268505345, 268505360, 268505361,
		269484032, 269484033, 269484048, 269484049, 269484288, 269484289, 269484304, 269484305, 269488128, 269488129,
		269488144, 269488145, 269488384, 269488385, 269488400, 269488401, 269549568, 269549569, 269549584, 269549585,
		269549824, 269549825, 269549840, 269549841, 269553664, 269553665, 269553680, 269553681, 269553920, 269553921,
		269553936, 269553937, 285212672, 285212673, 285212688, 285212689, 285212928, 285212929, 285212944, 285212945,
		285216768, 285216769, 285216784, 285216785, 285217024, 285217025, 285217040, 285217041, 285278208, 285278209,
		285278224, 285278225, 285278464, 285278465, 285278480, 285278481, 285282304, 285282305, 285282320, 285282321,
		285282560, 285282561, 285282576, 285282577, 286261248, 286261249, 286261264, 286261265, 286261504, 286261505,
		286261520, 286261521, 286265344, 286265345, 286265360, 286265361, 286265600, 286265601, 286265616, 286265617,
		286326784, 286326785, 286326800, 286326801, 286327040, 286327041, 286327056, 286327057, 286330880, 286330881,
		286330896, 286330897, 286331136, 286331137, 286331152, 286331153
	};

	private static readonly int[] INTERLEAVE5_TABLE = new int[128]
	{
		0, 1, 32, 33, 1024, 1025, 1056, 1057, 32768, 32769,
		32800, 32801, 33792, 33793, 33824, 33825, 1048576, 1048577, 1048608, 1048609,
		1049600, 1049601, 1049632, 1049633, 1081344, 1081345, 1081376, 1081377, 1082368, 1082369,
		1082400, 1082401, 33554432, 33554433, 33554464, 33554465, 33555456, 33555457, 33555488, 33555489,
		33587200, 33587201, 33587232, 33587233, 33588224, 33588225, 33588256, 33588257, 34603008, 34603009,
		34603040, 34603041, 34604032, 34604033, 34604064, 34604065, 34635776, 34635777, 34635808, 34635809,
		34636800, 34636801, 34636832, 34636833, 1073741824, 1073741825, 1073741856, 1073741857, 1073742848, 1073742849,
		1073742880, 1073742881, 1073774592, 1073774593, 1073774624, 1073774625, 1073775616, 1073775617, 1073775648, 1073775649,
		1074790400, 1074790401, 1074790432, 1074790433, 1074791424, 1074791425, 1074791456, 1074791457, 1074823168, 1074823169,
		1074823200, 1074823201, 1074824192, 1074824193, 1074824224, 1074824225, 1107296256, 1107296257, 1107296288, 1107296289,
		1107297280, 1107297281, 1107297312, 1107297313, 1107329024, 1107329025, 1107329056, 1107329057, 1107330048, 1107330049,
		1107330080, 1107330081, 1108344832, 1108344833, 1108344864, 1108344865, 1108345856, 1108345857, 1108345888, 1108345889,
		1108377600, 1108377601, 1108377632, 1108377633, 1108378624, 1108378625, 1108378656, 1108378657
	};

	private static readonly long[] INTERLEAVE7_TABLE = new long[512]
	{
		0L, 1L, 128L, 129L, 16384L, 16385L, 16512L, 16513L, 2097152L, 2097153L,
		2097280L, 2097281L, 2113536L, 2113537L, 2113664L, 2113665L, 268435456L, 268435457L, 268435584L, 268435585L,
		268451840L, 268451841L, 268451968L, 268451969L, 270532608L, 270532609L, 270532736L, 270532737L, 270548992L, 270548993L,
		270549120L, 270549121L, 34359738368L, 34359738369L, 34359738496L, 34359738497L, 34359754752L, 34359754753L, 34359754880L, 34359754881L,
		34361835520L, 34361835521L, 34361835648L, 34361835649L, 34361851904L, 34361851905L, 34361852032L, 34361852033L, 34628173824L, 34628173825L,
		34628173952L, 34628173953L, 34628190208L, 34628190209L, 34628190336L, 34628190337L, 34630270976L, 34630270977L, 34630271104L, 34630271105L,
		34630287360L, 34630287361L, 34630287488L, 34630287489L, 4398046511104L, 4398046511105L, 4398046511232L, 4398046511233L, 4398046527488L, 4398046527489L,
		4398046527616L, 4398046527617L, 4398048608256L, 4398048608257L, 4398048608384L, 4398048608385L, 4398048624640L, 4398048624641L, 4398048624768L, 4398048624769L,
		4398314946560L, 4398314946561L, 4398314946688L, 4398314946689L, 4398314962944L, 4398314962945L, 4398314963072L, 4398314963073L, 4398317043712L, 4398317043713L,
		4398317043840L, 4398317043841L, 4398317060096L, 4398317060097L, 4398317060224L, 4398317060225L, 4432406249472L, 4432406249473L, 4432406249600L, 4432406249601L,
		4432406265856L, 4432406265857L, 4432406265984L, 4432406265985L, 4432408346624L, 4432408346625L, 4432408346752L, 4432408346753L, 4432408363008L, 4432408363009L,
		4432408363136L, 4432408363137L, 4432674684928L, 4432674684929L, 4432674685056L, 4432674685057L, 4432674701312L, 4432674701313L, 4432674701440L, 4432674701441L,
		4432676782080L, 4432676782081L, 4432676782208L, 4432676782209L, 4432676798464L, 4432676798465L, 4432676798592L, 4432676798593L, 562949953421312L, 562949953421313L,
		562949953421440L, 562949953421441L, 562949953437696L, 562949953437697L, 562949953437824L, 562949953437825L, 562949955518464L, 562949955518465L, 562949955518592L, 562949955518593L,
		562949955534848L, 562949955534849L, 562949955534976L, 562949955534977L, 562950221856768L, 562950221856769L, 562950221856896L, 562950221856897L, 562950221873152L, 562950221873153L,
		562950221873280L, 562950221873281L, 562950223953920L, 562950223953921L, 562950223954048L, 562950223954049L, 562950223970304L, 562950223970305L, 562950223970432L, 562950223970433L,
		562984313159680L, 562984313159681L, 562984313159808L, 562984313159809L, 562984313176064L, 562984313176065L, 562984313176192L, 562984313176193L, 562984315256832L, 562984315256833L,
		562984315256960L, 562984315256961L, 562984315273216L, 562984315273217L, 562984315273344L, 562984315273345L, 562984581595136L, 562984581595137L, 562984581595264L, 562984581595265L,
		562984581611520L, 562984581611521L, 562984581611648L, 562984581611649L, 562984583692288L, 562984583692289L, 562984583692416L, 562984583692417L, 562984583708672L, 562984583708673L,
		562984583708800L, 562984583708801L, 567347999932416L, 567347999932417L, 567347999932544L, 567347999932545L, 567347999948800L, 567347999948801L, 567347999948928L, 567347999948929L,
		567348002029568L, 567348002029569L, 567348002029696L, 567348002029697L, 567348002045952L, 567348002045953L, 567348002046080L, 567348002046081L, 567348268367872L, 567348268367873L,
		567348268368000L, 567348268368001L, 567348268384256L, 567348268384257L, 567348268384384L, 567348268384385L, 567348270465024L, 567348270465025L, 567348270465152L, 567348270465153L,
		567348270481408L, 567348270481409L, 567348270481536L, 567348270481537L, 567382359670784L, 567382359670785L, 567382359670912L, 567382359670913L, 567382359687168L, 567382359687169L,
		567382359687296L, 567382359687297L, 567382361767936L, 567382361767937L, 567382361768064L, 567382361768065L, 567382361784320L, 567382361784321L, 567382361784448L, 567382361784449L,
		567382628106240L, 567382628106241L, 567382628106368L, 567382628106369L, 567382628122624L, 567382628122625L, 567382628122752L, 567382628122753L, 567382630203392L, 567382630203393L,
		567382630203520L, 567382630203521L, 567382630219776L, 567382630219777L, 567382630219904L, 567382630219905L, 72057594037927936L, 72057594037927937L, 72057594037928064L, 72057594037928065L,
		72057594037944320L, 72057594037944321L, 72057594037944448L, 72057594037944449L, 72057594040025088L, 72057594040025089L, 72057594040025216L, 72057594040025217L, 72057594040041472L, 72057594040041473L,
		72057594040041600L, 72057594040041601L, 72057594306363392L, 72057594306363393L, 72057594306363520L, 72057594306363521L, 72057594306379776L, 72057594306379777L, 72057594306379904L, 72057594306379905L,
		72057594308460544L, 72057594308460545L, 72057594308460672L, 72057594308460673L, 72057594308476928L, 72057594308476929L, 72057594308477056L, 72057594308477057L, 72057628397666304L, 72057628397666305L,
		72057628397666432L, 72057628397666433L, 72057628397682688L, 72057628397682689L, 72057628397682816L, 72057628397682817L, 72057628399763456L, 72057628399763457L, 72057628399763584L, 72057628399763585L,
		72057628399779840L, 72057628399779841L, 72057628399779968L, 72057628399779969L, 72057628666101760L, 72057628666101761L, 72057628666101888L, 72057628666101889L, 72057628666118144L, 72057628666118145L,
		72057628666118272L, 72057628666118273L, 72057628668198912L, 72057628668198913L, 72057628668199040L, 72057628668199041L, 72057628668215296L, 72057628668215297L, 72057628668215424L, 72057628668215425L,
		72061992084439040L, 72061992084439041L, 72061992084439168L, 72061992084439169L, 72061992084455424L, 72061992084455425L, 72061992084455552L, 72061992084455553L, 72061992086536192L, 72061992086536193L,
		72061992086536320L, 72061992086536321L, 72061992086552576L, 72061992086552577L, 72061992086552704L, 72061992086552705L, 72061992352874496L, 72061992352874497L, 72061992352874624L, 72061992352874625L,
		72061992352890880L, 72061992352890881L, 72061992352891008L, 72061992352891009L, 72061992354971648L, 72061992354971649L, 72061992354971776L, 72061992354971777L, 72061992354988032L, 72061992354988033L,
		72061992354988160L, 72061992354988161L, 72062026444177408L, 72062026444177409L, 72062026444177536L, 72062026444177537L, 72062026444193792L, 72062026444193793L, 72062026444193920L, 72062026444193921L,
		72062026446274560L, 72062026446274561L, 72062026446274688L, 72062026446274689L, 72062026446290944L, 72062026446290945L, 72062026446291072L, 72062026446291073L, 72062026712612864L, 72062026712612865L,
		72062026712612992L, 72062026712612993L, 72062026712629248L, 72062026712629249L, 72062026712629376L, 72062026712629377L, 72062026714710016L, 72062026714710017L, 72062026714710144L, 72062026714710145L,
		72062026714726400L, 72062026714726401L, 72062026714726528L, 72062026714726529L, 72620543991349248L, 72620543991349249L, 72620543991349376L, 72620543991349377L, 72620543991365632L, 72620543991365633L,
		72620543991365760L, 72620543991365761L, 72620543993446400L, 72620543993446401L, 72620543993446528L, 72620543993446529L, 72620543993462784L, 72620543993462785L, 72620543993462912L, 72620543993462913L,
		72620544259784704L, 72620544259784705L, 72620544259784832L, 72620544259784833L, 72620544259801088L, 72620544259801089L, 72620544259801216L, 72620544259801217L, 72620544261881856L, 72620544261881857L,
		72620544261881984L, 72620544261881985L, 72620544261898240L, 72620544261898241L, 72620544261898368L, 72620544261898369L, 72620578351087616L, 72620578351087617L, 72620578351087744L, 72620578351087745L,
		72620578351104000L, 72620578351104001L, 72620578351104128L, 72620578351104129L, 72620578353184768L, 72620578353184769L, 72620578353184896L, 72620578353184897L, 72620578353201152L, 72620578353201153L,
		72620578353201280L, 72620578353201281L, 72620578619523072L, 72620578619523073L, 72620578619523200L, 72620578619523201L, 72620578619539456L, 72620578619539457L, 72620578619539584L, 72620578619539585L,
		72620578621620224L, 72620578621620225L, 72620578621620352L, 72620578621620353L, 72620578621636608L, 72620578621636609L, 72620578621636736L, 72620578621636737L, 72624942037860352L, 72624942037860353L,
		72624942037860480L, 72624942037860481L, 72624942037876736L, 72624942037876737L, 72624942037876864L, 72624942037876865L, 72624942039957504L, 72624942039957505L, 72624942039957632L, 72624942039957633L,
		72624942039973888L, 72624942039973889L, 72624942039974016L, 72624942039974017L, 72624942306295808L, 72624942306295809L, 72624942306295936L, 72624942306295937L, 72624942306312192L, 72624942306312193L,
		72624942306312320L, 72624942306312321L, 72624942308392960L, 72624942308392961L, 72624942308393088L, 72624942308393089L, 72624942308409344L, 72624942308409345L, 72624942308409472L, 72624942308409473L,
		72624976397598720L, 72624976397598721L, 72624976397598848L, 72624976397598849L, 72624976397615104L, 72624976397615105L, 72624976397615232L, 72624976397615233L, 72624976399695872L, 72624976399695873L,
		72624976399696000L, 72624976399696001L, 72624976399712256L, 72624976399712257L, 72624976399712384L, 72624976399712385L, 72624976666034176L, 72624976666034177L, 72624976666034304L, 72624976666034305L,
		72624976666050560L, 72624976666050561L, 72624976666050688L, 72624976666050689L, 72624976668131328L, 72624976668131329L, 72624976668131456L, 72624976668131457L, 72624976668147712L, 72624976668147713L,
		72624976668147840L, 72624976668147841L
	};

	internal static readonly byte[] BitLengths = new byte[256]
	{
		0, 1, 2, 2, 3, 3, 3, 3, 4, 4,
		4, 4, 4, 4, 4, 4, 5, 5, 5, 5,
		5, 5, 5, 5, 5, 5, 5, 5, 5, 5,
		5, 5, 6, 6, 6, 6, 6, 6, 6, 6,
		6, 6, 6, 6, 6, 6, 6, 6, 6, 6,
		6, 6, 6, 6, 6, 6, 6, 6, 6, 6,
		6, 6, 6, 6, 7, 7, 7, 7, 7, 7,
		7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
		7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
		7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
		7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
		7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
		7, 7, 7, 7, 7, 7, 7, 7, 8, 8,
		8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
		8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
		8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
		8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
		8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
		8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
		8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
		8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
		8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
		8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
		8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
		8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
		8, 8, 8, 8, 8, 8
	};

	private long[] m_ints;

	public int Length => m_ints.Length;

	public LongArray(int intLen)
	{
		m_ints = new long[intLen];
	}

	public LongArray(long[] ints)
	{
		m_ints = ints;
	}

	public LongArray(long[] ints, int off, int len)
	{
		if (off == 0 && len == ints.Length)
		{
			m_ints = ints;
			return;
		}
		m_ints = new long[len];
		Array.Copy(ints, off, m_ints, 0, len);
	}

	public LongArray(BigInteger bigInt)
	{
		if (bigInt == null || bigInt.SignValue < 0)
		{
			throw new ArgumentException("invalid F2m field value", "bigInt");
		}
		if (bigInt.SignValue == 0)
		{
			long[] array = (m_ints = new long[1]);
			return;
		}
		byte[] array2 = bigInt.ToByteArray();
		int num = array2.Length;
		int num2 = 0;
		if (array2[0] == 0)
		{
			num--;
			num2 = 1;
		}
		int num3 = (num + 7) / 8;
		m_ints = new long[num3];
		int num4 = num3 - 1;
		int num5 = num % 8 + num2;
		long num6 = 0L;
		int i = num2;
		if (num2 < num5)
		{
			for (; i < num5; i++)
			{
				num6 <<= 8;
				uint num7 = array2[i];
				num6 |= num7;
			}
			m_ints[num4--] = num6;
		}
		while (num4 >= 0)
		{
			num6 = 0L;
			for (int j = 0; j < 8; j++)
			{
				num6 <<= 8;
				uint num8 = array2[i++];
				num6 |= num8;
			}
			m_ints[num4] = num6;
			num4--;
		}
	}

	internal void CopyTo(long[] z, int zOff)
	{
		Array.Copy(m_ints, 0, z, zOff, m_ints.Length);
	}

	public bool IsOne()
	{
		long[] ints = m_ints;
		if (ints[0] != 1)
		{
			return false;
		}
		for (int i = 1; i < ints.Length; i++)
		{
			if (ints[i] != 0)
			{
				return false;
			}
		}
		return true;
	}

	public bool IsZero()
	{
		long[] ints = m_ints;
		for (int i = 0; i < ints.Length; i++)
		{
			if (ints[i] != 0)
			{
				return false;
			}
		}
		return true;
	}

	public int GetUsedLength()
	{
		return GetUsedLengthFrom(m_ints.Length);
	}

	public int GetUsedLengthFrom(int from)
	{
		long[] ints = m_ints;
		from = System.Math.Min(from, ints.Length);
		if (from < 1)
		{
			return 0;
		}
		if (ints[0] != 0)
		{
			while (ints[--from] == 0)
			{
			}
			return from + 1;
		}
		do
		{
			if (ints[--from] != 0)
			{
				return from + 1;
			}
		}
		while (from > 0);
		return 0;
	}

	public int Degree()
	{
		int num = m_ints.Length;
		long num2;
		do
		{
			if (num == 0)
			{
				return 0;
			}
			num2 = m_ints[--num];
		}
		while (num2 == 0);
		return (num << 6) + BitLength(num2);
	}

	private int DegreeFrom(int limit)
	{
		int num = (int)((uint)(limit + 62) >> 6);
		long num2;
		do
		{
			if (num == 0)
			{
				return 0;
			}
			num2 = m_ints[--num];
		}
		while (num2 == 0);
		return (num << 6) + BitLength(num2);
	}

	private static int BitLength(long w)
	{
		int num = (int)((ulong)w >> 32);
		int num2;
		if (num == 0)
		{
			num = (int)w;
			num2 = 0;
		}
		else
		{
			num2 = 32;
		}
		int num3 = (int)((uint)num >> 16);
		int num4;
		if (num3 == 0)
		{
			num3 = (int)((uint)num >> 8);
			num4 = ((num3 == 0) ? BitLengths[num] : (8 + BitLengths[num3]));
		}
		else
		{
			int num5 = (int)((uint)num3 >> 8);
			num4 = ((num5 == 0) ? (16 + BitLengths[num3]) : (24 + BitLengths[num5]));
		}
		return num2 + num4;
	}

	private long[] ResizedInts(int newLen)
	{
		long[] array = new long[newLen];
		Array.Copy(m_ints, 0, array, 0, System.Math.Min(m_ints.Length, newLen));
		return array;
	}

	public BigInteger ToBigInteger()
	{
		int usedLength = GetUsedLength();
		if (usedLength == 0)
		{
			return BigInteger.Zero;
		}
		long num = m_ints[usedLength - 1];
		byte[] array = new byte[8];
		int num2 = 0;
		bool flag = false;
		for (int num3 = 7; num3 >= 0; num3--)
		{
			byte b = (byte)((ulong)num >> 8 * num3);
			if (flag || b != 0)
			{
				flag = true;
				array[num2++] = b;
			}
		}
		int num4 = 8 * (usedLength - 1) + num2;
		byte[] array2 = new byte[num4];
		for (int i = 0; i < num2; i++)
		{
			array2[i] = array[i];
		}
		for (int num5 = usedLength - 2; num5 >= 0; num5--)
		{
			long num6 = m_ints[num5];
			for (int num7 = 7; num7 >= 0; num7--)
			{
				array2[num2++] = (byte)((ulong)num6 >> 8 * num7);
			}
		}
		return new BigInteger(1, array2);
	}

	private static long ShiftUp(long[] x, int xOff, int count, int shift)
	{
		int num = 64 - shift;
		long num2 = 0L;
		for (int i = 0; i < count; i++)
		{
			long num3 = x[xOff + i];
			x[xOff + i] = (num3 << shift) | num2;
			num2 = (long)((ulong)num3 >> num);
		}
		return num2;
	}

	private static long ShiftUp(long[] x, int xOff, long[] z, int zOff, int count, int shift)
	{
		int num = 64 - shift;
		long num2 = 0L;
		for (int i = 0; i < count; i++)
		{
			long num3 = x[xOff + i];
			z[zOff + i] = (num3 << shift) | num2;
			num2 = (long)((ulong)num3 >> num);
		}
		return num2;
	}

	public LongArray AddOne()
	{
		if (m_ints.Length == 0)
		{
			return new LongArray(new long[1] { 1L });
		}
		int newLen = System.Math.Max(1, GetUsedLength());
		long[] array = ResizedInts(newLen);
		long[] array2;
		(array2 = array)[0] = array2[0] ^ 1;
		return new LongArray(array);
	}

	private void AddShiftedByBitsSafe(LongArray other, int otherDegree, int bits)
	{
		int num = (int)((uint)(otherDegree + 63) >> 6);
		int num2 = (int)((uint)bits >> 6);
		int num3 = bits & 0x3F;
		if (num3 == 0)
		{
			Add(m_ints, num2, other.m_ints, 0, num);
			return;
		}
		long num4 = AddShiftedUp(m_ints, num2, other.m_ints, 0, num, num3);
		if (num4 != 0)
		{
			long[] ints;
			long[] array = (ints = m_ints);
			int num5 = num + num2;
			nint num6 = num5;
			array[num5] = ints[num6] ^ num4;
		}
	}

	private static long AddShiftedUp(long[] x, int xOff, long[] y, int yOff, int count, int shift)
	{
		int num = 64 - shift;
		long num2 = 0L;
		for (int i = 0; i < count; i++)
		{
			long num3 = y[yOff + i];
			long[] array;
			long[] array2 = (array = x);
			int num4 = xOff + i;
			nint num5 = num4;
			array2[num4] = array[num5] ^ ((num3 << shift) | num2);
			num2 = (long)((ulong)num3 >> num);
		}
		return num2;
	}

	private static long AddShiftedDown(long[] x, int xOff, long[] y, int yOff, int count, int shift)
	{
		int num = 64 - shift;
		long num2 = 0L;
		int num3 = count;
		while (--num3 >= 0)
		{
			long num4 = y[yOff + num3];
			long[] array;
			long[] array2 = (array = x);
			int num5 = xOff + num3;
			nint num6 = num5;
			array2[num5] = array[num6] ^ ((long)((ulong)num4 >> shift) | num2);
			num2 = num4 << num;
		}
		return num2;
	}

	public void AddShiftedByWords(LongArray other, int words)
	{
		int usedLength = other.GetUsedLength();
		if (usedLength != 0)
		{
			int num = usedLength + words;
			if (num > m_ints.Length)
			{
				m_ints = ResizedInts(num);
			}
			Add(m_ints, words, other.m_ints, 0, usedLength);
		}
	}

	private static void Add(long[] x, int xOff, long[] y, int yOff, int count)
	{
		for (int i = 0; i < count; i++)
		{
			long[] array;
			long[] array2 = (array = x);
			int num = xOff + i;
			nint num2 = num;
			array2[num] = array[num2] ^ y[yOff + i];
		}
	}

	private static void Add(long[] x, int xOff, long[] y, int yOff, long[] z, int zOff, int count)
	{
		for (int i = 0; i < count; i++)
		{
			z[zOff + i] = x[xOff + i] ^ y[yOff + i];
		}
	}

	private static void AddBoth(long[] x, int xOff, long[] y1, int y1Off, long[] y2, int y2Off, int count)
	{
		for (int i = 0; i < count; i++)
		{
			long[] array;
			long[] array2 = (array = x);
			int num = xOff + i;
			nint num2 = num;
			array2[num] = array[num2] ^ (y1[y1Off + i] ^ y2[y2Off + i]);
		}
	}

	private static void Distribute(long[] x, int src, int dst1, int dst2, int count)
	{
		for (int i = 0; i < count; i++)
		{
			long num = x[src + i];
			long[] array;
			long[] array2 = (array = x);
			int num2 = dst1 + i;
			nint num3 = num2;
			array2[num2] = array[num3] ^ num;
			long[] array3 = (array = x);
			int num4 = dst2 + i;
			num3 = num4;
			array3[num4] = array[num3] ^ num;
		}
	}

	private static void FlipWord(long[] buf, int off, int bit, long word)
	{
		int num = off + (int)((uint)bit >> 6);
		int num2 = bit & 0x3F;
		long[] array;
		nint num4;
		if (num2 == 0)
		{
			long[] array2 = (array = buf);
			int num3 = num;
			num4 = num3;
			array2[num3] = array[num4] ^ word;
			return;
		}
		long[] array3 = (array = buf);
		int num5 = num;
		num4 = num5;
		array3[num5] = array[num4] ^ (word << num2);
		word = (long)((ulong)word >> 64 - num2);
		if (word != 0)
		{
			long[] array4 = (array = buf);
			int num6 = ++num;
			num4 = num6;
			array4[num6] = array[num4] ^ word;
		}
	}

	public bool TestBitZero()
	{
		if (m_ints.Length > 0)
		{
			return (m_ints[0] & 1) != 0;
		}
		return false;
	}

	private static bool TestBit(long[] buf, int off, int n)
	{
		int num = (int)((uint)n >> 6);
		int num2 = n & 0x3F;
		long num3 = 1L << num2;
		return (buf[off + num] & num3) != 0;
	}

	private static void FlipBit(long[] buf, int off, int n)
	{
		int num = (int)((uint)n >> 6);
		int num2 = n & 0x3F;
		long num3 = 1L << num2;
		long[] array;
		long[] array2 = (array = buf);
		int num4 = off + num;
		nint num5 = num4;
		array2[num4] = array[num5] ^ num3;
	}

	private static void MultiplyWord(long a, long[] b, int bLen, long[] c, int cOff)
	{
		if ((a & 1) != 0)
		{
			Add(c, cOff, b, 0, bLen);
		}
		int num = 1;
		while ((a = (long)((ulong)a >> 1)) != 0)
		{
			if ((a & 1) != 0)
			{
				long num2 = AddShiftedUp(c, cOff, b, 0, bLen, num);
				if (num2 != 0)
				{
					long[] array;
					long[] array2 = (array = c);
					int num3 = cOff + bLen;
					nint num4 = num3;
					array2[num3] = array[num4] ^ num2;
				}
			}
			num++;
		}
	}

	public LongArray ModMultiplyLD(LongArray other, int m, int[] ks)
	{
		int num = Degree();
		if (num == 0)
		{
			return this;
		}
		int num2 = other.Degree();
		if (num2 == 0)
		{
			return other;
		}
		LongArray longArray = this;
		LongArray longArray2 = other;
		if (num > num2)
		{
			longArray = other;
			longArray2 = this;
			int num3 = num;
			num = num2;
			num2 = num3;
		}
		int num4 = (int)((uint)(num + 63) >> 6);
		int num5 = (int)((uint)(num2 + 63) >> 6);
		int num6 = (int)((uint)(num + num2 + 62) >> 6);
		if (num4 == 1)
		{
			long num7 = longArray.m_ints[0];
			if (num7 == 1)
			{
				return longArray2;
			}
			long[] array = new long[num6];
			MultiplyWord(num7, longArray2.m_ints, num5, array, 0);
			return ReduceResult(array, 0, num6, m, ks);
		}
		int num8 = (int)((uint)(num2 + 7 + 63) >> 6);
		int[] array2 = new int[16];
		long[] array3 = new long[num8 << 4];
		int num9 = (array2[1] = num8);
		Array.Copy(longArray2.m_ints, 0, array3, num9, num5);
		for (int i = 2; i < 16; i++)
		{
			num9 = (array2[i] = num9 + num8);
			if ((i & 1) == 0)
			{
				ShiftUp(array3, (int)((uint)num9 >> 1), array3, num9, num8, 1);
			}
			else
			{
				Add(array3, num8, array3, num9 - num8, array3, num9, num8);
			}
		}
		long[] array4 = new long[array3.Length];
		ShiftUp(array3, 0, array4, 0, array3.Length, 4);
		long[] ints = longArray.m_ints;
		long[] array5 = new long[num6];
		int num10 = 15;
		for (int num11 = 56; num11 >= 0; num11 -= 8)
		{
			for (int j = 1; j < num4; j += 2)
			{
				int num12 = (int)((ulong)ints[j] >> num11);
				int num13 = num12 & num10;
				int num14 = (int)((uint)num12 >> 4) & num10;
				AddBoth(array5, j - 1, array3, array2[num13], array4, array2[num14], num8);
			}
			ShiftUp(array5, 0, num6, 8);
		}
		for (int num15 = 56; num15 >= 0; num15 -= 8)
		{
			for (int k = 0; k < num4; k += 2)
			{
				int num16 = (int)((ulong)ints[k] >> num15);
				int num17 = num16 & num10;
				int num18 = (int)((uint)num16 >> 4) & num10;
				AddBoth(array5, k, array3, array2[num17], array4, array2[num18], num8);
			}
			if (num15 > 0)
			{
				ShiftUp(array5, 0, num6, 8);
			}
		}
		return ReduceResult(array5, 0, num6, m, ks);
	}

	public LongArray ModMultiply(LongArray other, int m, int[] ks)
	{
		int num = Degree();
		if (num == 0)
		{
			return this;
		}
		int num2 = other.Degree();
		if (num2 == 0)
		{
			return other;
		}
		LongArray longArray = this;
		LongArray longArray2 = other;
		if (num > num2)
		{
			longArray = other;
			longArray2 = this;
			int num3 = num;
			num = num2;
			num2 = num3;
		}
		int num4 = (int)((uint)(num + 63) >> 6);
		int num5 = (int)((uint)(num2 + 63) >> 6);
		int num6 = (int)((uint)(num + num2 + 62) >> 6);
		if (num4 == 1)
		{
			long num7 = longArray.m_ints[0];
			if (num7 == 1)
			{
				return longArray2;
			}
			long[] array = new long[num6];
			MultiplyWord(num7, longArray2.m_ints, num5, array, 0);
			return ReduceResult(array, 0, num6, m, ks);
		}
		int num8 = (int)((uint)(num2 + 7 + 63) >> 6);
		int[] array2 = new int[16];
		long[] array3 = new long[num8 << 4];
		int num9 = (array2[1] = num8);
		Array.Copy(longArray2.m_ints, 0, array3, num9, num5);
		for (int i = 2; i < 16; i++)
		{
			num9 = (array2[i] = num9 + num8);
			if ((i & 1) == 0)
			{
				ShiftUp(array3, (int)((uint)num9 >> 1), array3, num9, num8, 1);
			}
			else
			{
				Add(array3, num8, array3, num9 - num8, array3, num9, num8);
			}
		}
		long[] array4 = new long[array3.Length];
		ShiftUp(array3, 0, array4, 0, array3.Length, 4);
		long[] ints = longArray.m_ints;
		long[] array5 = new long[num6 << 3];
		int num10 = 15;
		for (int j = 0; j < num4; j++)
		{
			long num11 = ints[j];
			int num12 = j;
			while (true)
			{
				int num13 = (int)num11 & num10;
				num11 = (long)((ulong)num11 >> 4);
				int num14 = (int)num11 & num10;
				AddBoth(array5, num12, array3, array2[num13], array4, array2[num14], num8);
				num11 = (long)((ulong)num11 >> 4);
				if (num11 == 0)
				{
					break;
				}
				num12 += num6;
			}
		}
		int num15 = array5.Length;
		while ((num15 -= num6) != 0)
		{
			AddShiftedUp(array5, num15 - num6, array5, num15, num6, 8);
		}
		return ReduceResult(array5, 0, num6, m, ks);
	}

	public LongArray ModMultiplyAlt(LongArray other, int m, int[] ks)
	{
		int num = Degree();
		if (num == 0)
		{
			return this;
		}
		int num2 = other.Degree();
		if (num2 == 0)
		{
			return other;
		}
		LongArray longArray = this;
		LongArray longArray2 = other;
		if (num > num2)
		{
			longArray = other;
			longArray2 = this;
			int num3 = num;
			num = num2;
			num2 = num3;
		}
		int num4 = (int)((uint)(num + 63) >> 6);
		int num5 = (int)((uint)(num2 + 63) >> 6);
		int num6 = (int)((uint)(num + num2 + 62) >> 6);
		if (num4 == 1)
		{
			long num7 = longArray.m_ints[0];
			if (num7 == 1)
			{
				return longArray2;
			}
			long[] array = new long[num6];
			MultiplyWord(num7, longArray2.m_ints, num5, array, 0);
			return ReduceResult(array, 0, num6, m, ks);
		}
		int num8 = 4;
		int num9 = 16;
		int num10 = 64;
		int num11 = 8;
		int num12 = ((num10 < 64) ? num9 : (num9 - 1));
		int num13 = (int)((uint)(num2 + num12 + 63) >> 6);
		int num14 = num13 * num11;
		int num15 = num8 * num11;
		int[] array2 = new int[1 << num8];
		int num16 = (array2[1] = (array2[0] = num4) + num14);
		for (int i = 2; i < array2.Length; i++)
		{
			num16 = (array2[i] = num16 + num6);
		}
		num16 += num6;
		num16++;
		long[] array3 = new long[num16];
		Interleave(longArray.m_ints, 0, array3, 0, num4, num8);
		int num17 = num4;
		Array.Copy(longArray2.m_ints, 0, array3, num17, num5);
		for (int j = 1; j < num11; j++)
		{
			ShiftUp(array3, num4, array3, num17 += num13, num13, j);
		}
		int num18 = (1 << num8) - 1;
		int num19 = 0;
		while (true)
		{
			int num20 = 0;
			do
			{
				long num21 = (long)((ulong)array3[num20] >> num19);
				int num22 = 0;
				int num23 = num4;
				while (true)
				{
					int num24 = (int)num21 & num18;
					if (num24 != 0)
					{
						Add(array3, num20 + array2[num24], array3, num23, num13);
					}
					if (++num22 == num11)
					{
						break;
					}
					num23 += num13;
					num21 = (long)((ulong)num21 >> num8);
				}
			}
			while (++num20 < num4);
			if ((num19 += num15) >= num10)
			{
				if (num19 >= 64)
				{
					break;
				}
				num19 = 64 - num8;
				num18 &= num18 << num10 - num19;
			}
			ShiftUp(array3, num4, num14, num11);
		}
		int num25 = array2.Length;
		while (--num25 > 1)
		{
			if (((ulong)num25 & 1uL) == 0)
			{
				AddShiftedUp(array3, array2[(uint)num25 >> 1], array3, array2[num25], num6, num9);
			}
			else
			{
				Distribute(array3, array2[num25], array2[num25 - 1], array2[1], num6);
			}
		}
		return ReduceResult(array3, array2[1], num6, m, ks);
	}

	public LongArray ModReduce(int m, int[] ks)
	{
		long[] array = Arrays.Clone(m_ints);
		int len = ReduceInPlace(array, 0, array.Length, m, ks);
		return new LongArray(array, 0, len);
	}

	public LongArray Multiply(LongArray other, int m, int[] ks)
	{
		int num = Degree();
		if (num == 0)
		{
			return this;
		}
		int num2 = other.Degree();
		if (num2 == 0)
		{
			return other;
		}
		LongArray longArray = this;
		LongArray longArray2 = other;
		if (num > num2)
		{
			longArray = other;
			longArray2 = this;
			int num3 = num;
			num = num2;
			num2 = num3;
		}
		int num4 = (int)((uint)(num + 63) >> 6);
		int num5 = (int)((uint)(num2 + 63) >> 6);
		int num6 = (int)((uint)(num + num2 + 62) >> 6);
		if (num4 == 1)
		{
			long num7 = longArray.m_ints[0];
			if (num7 == 1)
			{
				return longArray2;
			}
			long[] array = new long[num6];
			MultiplyWord(num7, longArray2.m_ints, num5, array, 0);
			return new LongArray(array, 0, num6);
		}
		int num8 = (int)((uint)(num2 + 7 + 63) >> 6);
		int[] array2 = new int[16];
		long[] array3 = new long[num8 << 4];
		int num9 = (array2[1] = num8);
		Array.Copy(longArray2.m_ints, 0, array3, num9, num5);
		for (int i = 2; i < 16; i++)
		{
			num9 = (array2[i] = num9 + num8);
			if ((i & 1) == 0)
			{
				ShiftUp(array3, (int)((uint)num9 >> 1), array3, num9, num8, 1);
			}
			else
			{
				Add(array3, num8, array3, num9 - num8, array3, num9, num8);
			}
		}
		long[] array4 = new long[array3.Length];
		ShiftUp(array3, 0, array4, 0, array3.Length, 4);
		long[] ints = longArray.m_ints;
		long[] array5 = new long[num6 << 3];
		int num10 = 15;
		for (int j = 0; j < num4; j++)
		{
			long num11 = ints[j];
			int num12 = j;
			while (true)
			{
				int num13 = (int)num11 & num10;
				num11 = (long)((ulong)num11 >> 4);
				int num14 = (int)num11 & num10;
				AddBoth(array5, num12, array3, array2[num13], array4, array2[num14], num8);
				num11 = (long)((ulong)num11 >> 4);
				if (num11 == 0)
				{
					break;
				}
				num12 += num6;
			}
		}
		int num15 = array5.Length;
		while ((num15 -= num6) != 0)
		{
			AddShiftedUp(array5, num15 - num6, array5, num15, num6, 8);
		}
		return new LongArray(array5, 0, num6);
	}

	public void Reduce(int m, int[] ks)
	{
		long[] ints = m_ints;
		int num = ReduceInPlace(ints, 0, ints.Length, m, ks);
		if (num < ints.Length)
		{
			m_ints = new long[num];
			Array.Copy(ints, 0, m_ints, 0, num);
		}
	}

	private static LongArray ReduceResult(long[] buf, int off, int len, int m, int[] ks)
	{
		int len2 = ReduceInPlace(buf, off, len, m, ks);
		return new LongArray(buf, off, len2);
	}

	private static int ReduceInPlace(long[] buf, int off, int len, int m, int[] ks)
	{
		int num = m + 63 >> 6;
		if (len < num)
		{
			return len;
		}
		int num2 = System.Math.Min(len << 6, (m << 1) - 1);
		int num3;
		for (num3 = (len << 6) - num2; num3 >= 64; num3 -= 64)
		{
			len--;
		}
		int num4 = ks.Length;
		int num5 = ks[num4 - 1];
		int num6 = ((num4 > 1) ? ks[num4 - 2] : 0);
		int num7 = System.Math.Max(m, num5 + 64);
		int num8 = num3 + System.Math.Min(num2 - num7, m - num6) >> 6;
		if (num8 > 1)
		{
			int num9 = len - num8;
			ReduceVectorWise(buf, off, len, num9, m, ks);
			while (len > num9)
			{
				buf[off + --len] = 0L;
			}
			num2 = num9 << 6;
		}
		if (num2 > num7)
		{
			ReduceWordWise(buf, off, len, num7, m, ks);
			num2 = num7;
		}
		if (num2 > m)
		{
			ReduceBitWise(buf, off, num2, m, ks);
		}
		return num;
	}

	private static void ReduceBitWise(long[] buf, int off, int BitLength, int m, int[] ks)
	{
		while (--BitLength >= m)
		{
			if (TestBit(buf, off, BitLength))
			{
				ReduceBit(buf, off, BitLength, m, ks);
			}
		}
	}

	private static void ReduceBit(long[] buf, int off, int bit, int m, int[] ks)
	{
		FlipBit(buf, off, bit);
		int num = bit - m;
		int num2 = ks.Length;
		while (--num2 >= 0)
		{
			FlipBit(buf, off, ks[num2] + num);
		}
		FlipBit(buf, off, num);
	}

	private static void ReduceWordWise(long[] buf, int off, int len, int toBit, int m, int[] ks)
	{
		int num = (int)((uint)toBit >> 6);
		while (--len > num)
		{
			long num2 = buf[off + len];
			if (num2 != 0)
			{
				buf[off + len] = 0L;
				ReduceWord(buf, off, len << 6, num2, m, ks);
			}
		}
		int num3 = toBit & 0x3F;
		long num4 = (long)((ulong)buf[off + num] >> num3);
		if (num4 != 0)
		{
			long[] array;
			long[] array2 = (array = buf);
			int num5 = off + num;
			nint num6 = num5;
			array2[num5] = array[num6] ^ (num4 << num3);
			ReduceWord(buf, off, toBit, num4, m, ks);
		}
	}

	private static void ReduceWord(long[] buf, int off, int bit, long word, int m, int[] ks)
	{
		int num = bit - m;
		int num2 = ks.Length;
		while (--num2 >= 0)
		{
			FlipWord(buf, off, num + ks[num2], word);
		}
		FlipWord(buf, off, num, word);
	}

	private static void ReduceVectorWise(long[] buf, int off, int len, int words, int m, int[] ks)
	{
		int num = (words << 6) - m;
		int num2 = ks.Length;
		while (--num2 >= 0)
		{
			FlipVector(buf, off, buf, off + words, len - words, num + ks[num2]);
		}
		FlipVector(buf, off, buf, off + words, len - words, num);
	}

	private static void FlipVector(long[] x, int xOff, long[] y, int yOff, int yLen, int bits)
	{
		xOff += (int)((uint)bits >> 6);
		bits &= 0x3F;
		if (bits == 0)
		{
			Add(x, xOff, y, yOff, yLen);
			return;
		}
		long num = AddShiftedDown(x, xOff + 1, y, yOff, yLen, 64 - bits);
		long[] array;
		long[] array2 = (array = x);
		int num2 = xOff;
		nint num3 = num2;
		array2[num2] = array[num3] ^ num;
	}

	public LongArray ModSquare(int m, int[] ks)
	{
		int usedLength = GetUsedLength();
		if (usedLength == 0)
		{
			return this;
		}
		int num = usedLength << 1;
		long[] array = new long[num];
		int num2 = 0;
		while (num2 < num)
		{
			long num3 = m_ints[(uint)num2 >> 1];
			array[num2++] = Interleave2_32to64((int)num3);
			array[num2++] = Interleave2_32to64((int)((ulong)num3 >> 32));
		}
		return new LongArray(array, 0, ReduceInPlace(array, 0, array.Length, m, ks));
	}

	public LongArray ModSquareN(int n, int m, int[] ks)
	{
		int num = GetUsedLength();
		if (num == 0)
		{
			return this;
		}
		int num2 = m + 63 >> 6;
		long[] array = new long[num2 << 1];
		Array.Copy(m_ints, 0, array, 0, num);
		while (--n >= 0)
		{
			SquareInPlace(array, num, m, ks);
			num = ReduceInPlace(array, 0, array.Length, m, ks);
		}
		return new LongArray(array, 0, num);
	}

	public LongArray Square(int m, int[] ks)
	{
		int usedLength = GetUsedLength();
		if (usedLength == 0)
		{
			return this;
		}
		int num = usedLength << 1;
		long[] array = new long[num];
		int num2 = 0;
		while (num2 < num)
		{
			long num3 = m_ints[(uint)num2 >> 1];
			array[num2++] = Interleave2_32to64((int)num3);
			array[num2++] = Interleave2_32to64((int)((ulong)num3 >> 32));
		}
		return new LongArray(array, 0, array.Length);
	}

	private static void SquareInPlace(long[] x, int xLen, int m, int[] ks)
	{
		int num = xLen << 1;
		while (--xLen >= 0)
		{
			long num2 = x[xLen];
			x[--num] = Interleave2_32to64((int)((ulong)num2 >> 32));
			x[--num] = Interleave2_32to64((int)num2);
		}
	}

	private static void Interleave(long[] x, int xOff, long[] z, int zOff, int count, int width)
	{
		switch (width)
		{
		case 3:
			Interleave3(x, xOff, z, zOff, count);
			break;
		case 5:
			Interleave5(x, xOff, z, zOff, count);
			break;
		case 7:
			Interleave7(x, xOff, z, zOff, count);
			break;
		default:
			Interleave2_n(x, xOff, z, zOff, count, BitLengths[width] - 1);
			break;
		}
	}

	private static void Interleave3(long[] x, int xOff, long[] z, int zOff, int count)
	{
		for (int i = 0; i < count; i++)
		{
			z[zOff + i] = Interleave3(x[xOff + i]);
		}
	}

	private static long Interleave3(long x)
	{
		long num = x & long.MinValue;
		return num | Interleave3_21to63((int)x & 0x1FFFFF) | (Interleave3_21to63((int)((ulong)x >> 21) & 0x1FFFFF) << 1) | (Interleave3_21to63((int)((ulong)x >> 42) & 0x1FFFFF) << 2);
	}

	private static long Interleave3_21to63(int x)
	{
		int num = INTERLEAVE3_TABLE[x & 0x7F];
		int num2 = INTERLEAVE3_TABLE[((uint)x >> 7) & 0x7F];
		int num3 = INTERLEAVE3_TABLE[(uint)x >> 14];
		return ((num3 & 0xFFFFFFFFu) << 42) | ((num2 & 0xFFFFFFFFu) << 21) | (num & 0xFFFFFFFFu);
	}

	private static void Interleave5(long[] x, int xOff, long[] z, int zOff, int count)
	{
		for (int i = 0; i < count; i++)
		{
			z[zOff + i] = Interleave5(x[xOff + i]);
		}
	}

	private static long Interleave5(long x)
	{
		return Interleave3_13to65((int)x & 0x1FFF) | (Interleave3_13to65((int)((ulong)x >> 13) & 0x1FFF) << 1) | (Interleave3_13to65((int)((ulong)x >> 26) & 0x1FFF) << 2) | (Interleave3_13to65((int)((ulong)x >> 39) & 0x1FFF) << 3) | (Interleave3_13to65((int)((ulong)x >> 52) & 0x1FFF) << 4);
	}

	private static long Interleave3_13to65(int x)
	{
		int num = INTERLEAVE5_TABLE[x & 0x7F];
		int num2 = INTERLEAVE5_TABLE[(uint)x >> 7];
		return ((num2 & 0xFFFFFFFFu) << 35) | (num & 0xFFFFFFFFu);
	}

	private static void Interleave7(long[] x, int xOff, long[] z, int zOff, int count)
	{
		for (int i = 0; i < count; i++)
		{
			z[zOff + i] = Interleave7(x[xOff + i]);
		}
	}

	private static long Interleave7(long x)
	{
		long num = x & long.MinValue;
		return num | INTERLEAVE7_TABLE[(int)x & 0x1FF] | (INTERLEAVE7_TABLE[(int)((ulong)x >> 9) & 0x1FF] << 1) | (INTERLEAVE7_TABLE[(int)((ulong)x >> 18) & 0x1FF] << 2) | (INTERLEAVE7_TABLE[(int)((ulong)x >> 27) & 0x1FF] << 3) | (INTERLEAVE7_TABLE[(int)((ulong)x >> 36) & 0x1FF] << 4) | (INTERLEAVE7_TABLE[(int)((ulong)x >> 45) & 0x1FF] << 5) | (INTERLEAVE7_TABLE[(int)((ulong)x >> 54) & 0x1FF] << 6);
	}

	private static void Interleave2_n(long[] x, int xOff, long[] z, int zOff, int count, int rounds)
	{
		for (int i = 0; i < count; i++)
		{
			z[zOff + i] = Interleave2_n(x[xOff + i], rounds);
		}
	}

	private static long Interleave2_n(long x, int rounds)
	{
		while (rounds > 1)
		{
			rounds -= 2;
			x = Interleave4_16to64((int)x & 0xFFFF) | (Interleave4_16to64((int)((ulong)x >> 16) & 0xFFFF) << 1) | (Interleave4_16to64((int)((ulong)x >> 32) & 0xFFFF) << 2) | (Interleave4_16to64((int)((ulong)x >> 48) & 0xFFFF) << 3);
		}
		if (rounds > 0)
		{
			x = Interleave2_32to64((int)x) | (Interleave2_32to64((int)((ulong)x >> 32)) << 1);
		}
		return x;
	}

	private static long Interleave4_16to64(int x)
	{
		int num = INTERLEAVE4_TABLE[x & 0xFF];
		int num2 = INTERLEAVE4_TABLE[(uint)x >> 8];
		return ((num2 & 0xFFFFFFFFu) << 32) | (num & 0xFFFFFFFFu);
	}

	private static long Interleave2_32to64(int x)
	{
		int num = INTERLEAVE2_TABLE[x & 0xFF] | (INTERLEAVE2_TABLE[((uint)x >> 8) & 0xFF] << 16);
		int num2 = INTERLEAVE2_TABLE[((uint)x >> 16) & 0xFF] | (INTERLEAVE2_TABLE[(uint)x >> 24] << 16);
		return ((num2 & 0xFFFFFFFFu) << 32) | (num & 0xFFFFFFFFu);
	}

	public LongArray ModInverse(int m, int[] ks)
	{
		int num = Degree();
		switch (num)
		{
		case 0:
			throw new InvalidOperationException();
		case 1:
			return this;
		default:
		{
			LongArray longArray = Copy();
			int intLen = m + 63 >> 6;
			LongArray longArray2 = new LongArray(intLen);
			ReduceBit(longArray2.m_ints, 0, m, m, ks);
			LongArray longArray3 = new LongArray(intLen);
			longArray3.m_ints[0] = 1L;
			LongArray longArray4 = new LongArray(intLen);
			int[] array = new int[2]
			{
				num,
				m + 1
			};
			LongArray[] array2 = new LongArray[2] { longArray, longArray2 };
			int[] array3 = new int[2] { 1, 0 };
			LongArray[] array4 = new LongArray[2] { longArray3, longArray4 };
			int num2 = 1;
			int num3 = array[num2];
			int num4 = array3[num2];
			int num5 = num3 - array[1 - num2];
			while (true)
			{
				if (num5 < 0)
				{
					num5 = -num5;
					array[num2] = num3;
					array3[num2] = num4;
					num2 = 1 - num2;
					num3 = array[num2];
					num4 = array3[num2];
				}
				array2[num2].AddShiftedByBitsSafe(array2[1 - num2], array[1 - num2], num5);
				int num6 = array2[num2].DegreeFrom(num3);
				if (num6 == 0)
				{
					break;
				}
				int num7 = array3[1 - num2];
				array4[num2].AddShiftedByBitsSafe(array4[1 - num2], num7, num5);
				num7 += num5;
				if (num7 > num4)
				{
					num4 = num7;
				}
				else if (num7 == num4)
				{
					num4 = array4[num2].DegreeFrom(num4);
				}
				num5 += num6 - num3;
				num3 = num6;
			}
			return array4[1 - num2];
		}
		}
	}

	public override bool Equals(object obj)
	{
		return Equals(obj as LongArray);
	}

	public virtual bool Equals(LongArray other)
	{
		if (this == other)
		{
			return true;
		}
		if (other == null)
		{
			return false;
		}
		int usedLength = GetUsedLength();
		if (other.GetUsedLength() != usedLength)
		{
			return false;
		}
		for (int i = 0; i < usedLength; i++)
		{
			if (m_ints[i] != other.m_ints[i])
			{
				return false;
			}
		}
		return true;
	}

	public override int GetHashCode()
	{
		int usedLength = GetUsedLength();
		int num = 1;
		for (int i = 0; i < usedLength; i++)
		{
			long num2 = m_ints[i];
			num *= 31;
			num ^= (int)num2;
			num *= 31;
			num ^= (int)((ulong)num2 >> 32);
		}
		return num;
	}

	public LongArray Copy()
	{
		return new LongArray(Arrays.Clone(m_ints));
	}

	public override string ToString()
	{
		int usedLength = GetUsedLength();
		if (usedLength == 0)
		{
			return "0";
		}
		StringBuilder stringBuilder = new StringBuilder(Convert.ToString(m_ints[--usedLength], 2));
		while (--usedLength >= 0)
		{
			string text = Convert.ToString(m_ints[usedLength], 2);
			int length = text.Length;
			if (length < 64)
			{
				stringBuilder.Append("0000000000000000000000000000000000000000000000000000000000000000".Substring(length));
			}
			stringBuilder.Append(text);
		}
		return stringBuilder.ToString();
	}
}
