using System;
using Org.BouncyCastle.Crypto.Parameters;

namespace Org.BouncyCastle.Crypto.Engines;

public class CamelliaEngine : IBlockCipher
{
	private const int BLOCK_SIZE = 16;

	private bool initialised = false;

	private bool _keyIs128;

	private uint[] subkey = new uint[96];

	private uint[] kw = new uint[8];

	private uint[] ke = new uint[12];

	private uint[] state = new uint[4];

	private static readonly uint[] SIGMA = new uint[12]
	{
		2694735487u, 1003262091u, 3061508184u, 1286239154u, 3337565999u, 3914302142u, 1426019237u, 4057165596u, 283453434u, 3731369245u,
		2958461122u, 3018244605u
	};

	private static readonly uint[] SBOX1_1110 = new uint[256]
	{
		1886416896u, 2189591040u, 741092352u, 3974949888u, 3014898432u, 656877312u, 3233857536u, 3857048832u, 3840205824u, 2240120064u,
		1465341696u, 892679424u, 3941263872u, 202116096u, 2930683392u, 1094795520u, 589505280u, 4025478912u, 1802201856u, 2475922176u,
		1162167552u, 421075200u, 2779096320u, 555819264u, 3991792896u, 235802112u, 1330597632u, 1313754624u, 488447232u, 1701143808u,
		2459079168u, 3183328512u, 2256963072u, 3099113472u, 2947526400u, 2408550144u, 2088532992u, 3958106880u, 522133248u, 3469659648u,
		1044266496u, 808464384u, 3705461760u, 1600085760u, 1583242752u, 3318072576u, 185273088u, 437918208u, 2795939328u, 3789676800u,
		960051456u, 3402287616u, 3587560704u, 1195853568u, 1566399744u, 1027423488u, 3654932736u, 16843008u, 1515870720u, 3604403712u,
		1364283648u, 1448498688u, 1819044864u, 1296911616u, 2341178112u, 218959104u, 2593823232u, 1717986816u, 4227595008u, 3435973632u,
		2964369408u, 757935360u, 1953788928u, 303174144u, 724249344u, 538976256u, 4042321920u, 2981212416u, 2223277056u, 2576980224u,
		3755990784u, 1280068608u, 3419130624u, 3267543552u, 875836416u, 2122219008u, 1987474944u, 84215040u, 1835887872u, 3082270464u,
		2846468352u, 825307392u, 3520188672u, 387389184u, 67372032u, 3621246720u, 336860160u, 1482184704u, 976894464u, 1633771776u,
		3739147776u, 454761216u, 286331136u, 471604224u, 842150400u, 252645120u, 2627509248u, 370546176u, 1397969664u, 404232192u,
		4076007936u, 572662272u, 4278124032u, 1145324544u, 3486502656u, 2998055424u, 3284386560u, 3048584448u, 2054846976u, 2442236160u,
		606348288u, 134744064u, 3907577856u, 2829625344u, 1616928768u, 4244438016u, 1768515840u, 1347440640u, 2863311360u, 3503345664u,
		2694881280u, 2105376000u, 2711724288u, 2307492096u, 1650614784u, 2543294208u, 1414812672u, 1532713728u, 505290240u, 2509608192u,
		3772833792u, 4294967040u, 1684300800u, 3537031680u, 269488128u, 3301229568u, 0u, 1212696576u, 2745410304u, 4160222976u,
		1970631936u, 3688618752u, 2324335104u, 50529024u, 3873891840u, 3671775744u, 151587072u, 1061109504u, 3722304768u, 2492765184u,
		2273806080u, 1549556736u, 2206434048u, 33686016u, 3452816640u, 1246382592u, 2425393152u, 858993408u, 1936945920u, 1734829824u,
		4143379968u, 4092850944u, 2644352256u, 2139062016u, 3217014528u, 3806519808u, 1381126656u, 2610666240u, 3638089728u, 640034304u,
		3368601600u, 926365440u, 3334915584u, 993737472u, 2172748032u, 2526451200u, 1869573888u, 1263225600u, 320017152u, 3200171520u,
		1667457792u, 774778368u, 3924420864u, 2038003968u, 2812782336u, 2358021120u, 2678038272u, 1852730880u, 3166485504u, 2391707136u,
		690563328u, 4126536960u, 4193908992u, 3065427456u, 791621376u, 4261281024u, 3031741440u, 1499027712u, 2021160960u, 2560137216u,
		101058048u, 1785358848u, 3890734848u, 1179010560u, 1903259904u, 3132799488u, 3570717696u, 623191296u, 2880154368u, 1111638528u,
		2290649088u, 2728567296u, 2374864128u, 4210752000u, 1920102912u, 117901056u, 3115956480u, 1431655680u, 4177065984u, 4008635904u,
		2896997376u, 168430080u, 909522432u, 1229539584u, 707406336u, 1751672832u, 1010580480u, 943208448u, 4059164928u, 2762253312u,
		1077952512u, 673720320u, 3553874688u, 2071689984u, 3149642496u, 3385444608u, 1128481536u, 3250700544u, 353703168u, 3823362816u,
		2913840384u, 4109693952u, 2004317952u, 3351758592u, 2155905024u, 2661195264u
	};

	private static readonly uint[] SBOX4_4404 = new uint[256]
	{
		1886388336u, 741081132u, 3014852787u, 3233808576u, 3840147684u, 1465319511u, 3941204202u, 2930639022u, 589496355u, 1802174571u,
		1162149957u, 2779054245u, 3991732461u, 1330577487u, 488439837u, 2459041938u, 2256928902u, 2947481775u, 2088501372u, 522125343u,
		1044250686u, 3705405660u, 1583218782u, 185270283u, 2795896998u, 960036921u, 3587506389u, 1566376029u, 3654877401u, 1515847770u,
		1364262993u, 1819017324u, 2341142667u, 2593783962u, 4227531003u, 2964324528u, 1953759348u, 724238379u, 4042260720u, 2223243396u,
		3755933919u, 3419078859u, 875823156u, 1987444854u, 1835860077u, 2846425257u, 3520135377u, 67371012u, 336855060u, 976879674u,
		3739091166u, 286326801u, 842137650u, 2627469468u, 1397948499u, 4075946226u, 4278059262u, 3486449871u, 3284336835u, 2054815866u,
		606339108u, 3907518696u, 1616904288u, 1768489065u, 2863268010u, 2694840480u, 2711683233u, 1650589794u, 1414791252u, 505282590u,
		3772776672u, 1684275300u, 269484048u, 0u, 2745368739u, 1970602101u, 2324299914u, 3873833190u, 151584777u, 3722248413u,
		2273771655u, 2206400643u, 3452764365u, 2425356432u, 1936916595u, 4143317238u, 2644312221u, 3216965823u, 1381105746u, 3638034648u,
		3368550600u, 3334865094u, 2172715137u, 1869545583u, 320012307u, 1667432547u, 3924361449u, 2812739751u, 2677997727u, 3166437564u,
		690552873u, 4193845497u, 791609391u, 3031695540u, 2021130360u, 101056518u, 3890675943u, 1903231089u, 3570663636u, 2880110763u,
		2290614408u, 2374828173u, 1920073842u, 3115909305u, 4177002744u, 2896953516u, 909508662u, 707395626u, 1010565180u, 4059103473u,
		1077936192u, 3553820883u, 3149594811u, 1128464451u, 353697813u, 2913796269u, 2004287607u, 2155872384u, 2189557890u, 3974889708u,
		656867367u, 3856990437u, 2240086149u, 892665909u, 202113036u, 1094778945u, 4025417967u, 2475884691u, 421068825u, 555810849u,
		235798542u, 1313734734u, 1701118053u, 3183280317u, 3099066552u, 2408513679u, 3958046955u, 3469607118u, 808452144u, 1600061535u,
		3318022341u, 437911578u, 3789619425u, 3402236106u, 1195835463u, 1027407933u, 16842753u, 3604349142u, 1448476758u, 1296891981u,
		218955789u, 1717960806u, 3435921612u, 757923885u, 303169554u, 538968096u, 2981167281u, 2576941209u, 1280049228u, 3267494082u,
		2122186878u, 84213765u, 3082223799u, 825294897u, 387383319u, 3621191895u, 1482162264u, 1633747041u, 454754331u, 471597084u,
		252641295u, 370540566u, 404226072u, 572653602u, 1145307204u, 2998010034u, 3048538293u, 2442199185u, 134742024u, 2829582504u,
		4244373756u, 1347420240u, 3503292624u, 2105344125u, 2307457161u, 2543255703u, 1532690523u, 2509570197u, 4294902015u, 3536978130u,
		3301179588u, 1212678216u, 4160159991u, 3688562907u, 50528259u, 3671720154u, 1061093439u, 2492727444u, 1549533276u, 33685506u,
		1246363722u, 858980403u, 1734803559u, 4092788979u, 2139029631u, 3806462178u, 2610626715u, 640024614u, 926351415u, 993722427u,
		2526412950u, 1263206475u, 3200123070u, 774766638u, 2037973113u, 2357985420u, 1852702830u, 2391670926u, 4126474485u, 3065381046u,
		4261216509u, 1499005017u, 2560098456u, 1785331818u, 1178992710u, 3132752058u, 623181861u, 1111621698u, 2728525986u, 4210688250u,
		117899271u, 1431634005u, 4008575214u, 168427530u, 1229520969u, 1751646312u, 943194168u, 2762211492u, 673710120u, 2071658619u,
		3385393353u, 3250651329u, 3823304931u, 4109631732u, 3351707847u, 2661154974u
	};

	private static readonly uint[] SBOX2_0222 = new uint[256]
	{
		14737632u, 328965u, 5789784u, 14277081u, 6776679u, 5131854u, 8487297u, 13355979u, 13224393u, 723723u,
		11447982u, 6974058u, 14013909u, 1579032u, 6118749u, 8553090u, 4605510u, 14671839u, 14079702u, 2565927u,
		9079434u, 3289650u, 4934475u, 4342338u, 14408667u, 1842204u, 10395294u, 10263708u, 3815994u, 13290186u,
		2434341u, 8092539u, 855309u, 7434609u, 6250335u, 2039583u, 16316664u, 14145495u, 4079166u, 10329501u,
		8158332u, 6316128u, 12171705u, 12500670u, 12369084u, 9145227u, 1447446u, 3421236u, 5066061u, 12829635u,
		7500402u, 9803157u, 11250603u, 9342606u, 12237498u, 8026746u, 11776947u, 131586u, 11842740u, 11382189u,
		10658466u, 11316396u, 14211288u, 10132122u, 1513239u, 1710618u, 3487029u, 13421772u, 16250871u, 10066329u,
		6381921u, 5921370u, 15263976u, 2368548u, 5658198u, 4210752u, 14803425u, 6513507u, 592137u, 3355443u,
		12566463u, 10000536u, 9934743u, 8750469u, 6842472u, 16579836u, 15527148u, 657930u, 14342874u, 7303023u,
		5460819u, 6447714u, 10724259u, 3026478u, 526344u, 11513775u, 2631720u, 11579568u, 7631988u, 12763842u,
		12434877u, 3552822u, 2236962u, 3684408u, 6579300u, 1973790u, 3750201u, 2894892u, 10921638u, 3158064u,
		15066597u, 4473924u, 16645629u, 8947848u, 10461087u, 6645093u, 8882055u, 7039851u, 16053492u, 2302755u,
		4737096u, 1052688u, 13750737u, 5329233u, 12632256u, 16382457u, 13816530u, 10526880u, 5592405u, 10592673u,
		4276545u, 16448250u, 4408131u, 1250067u, 12895428u, 3092271u, 11053224u, 11974326u, 3947580u, 2829099u,
		12698049u, 16777215u, 13158600u, 10855845u, 2105376u, 9013641u, 0u, 9474192u, 4671303u, 15724527u,
		15395562u, 12040119u, 1381653u, 394758u, 13487565u, 11908533u, 1184274u, 8289918u, 12303291u, 2697513u,
		986895u, 12105912u, 460551u, 263172u, 10197915u, 9737364u, 2171169u, 6710886u, 15132390u, 13553358u,
		15592941u, 15198183u, 3881787u, 16711422u, 8355711u, 12961221u, 10790052u, 3618615u, 11645361u, 5000268u,
		9539985u, 7237230u, 9276813u, 7763574u, 197379u, 2960685u, 14606046u, 9868950u, 2500134u, 8224125u,
		13027014u, 6052956u, 13882323u, 15921906u, 5197647u, 1644825u, 4144959u, 14474460u, 7960953u, 1907997u,
		5395026u, 15461355u, 15987699u, 7171437u, 6184542u, 16514043u, 6908265u, 11711154u, 15790320u, 3223857u,
		789516u, 13948116u, 13619151u, 9211020u, 14869218u, 7697781u, 11119017u, 4868682u, 5723991u, 8684676u,
		1118481u, 4539717u, 1776411u, 16119285u, 15000804u, 921102u, 7566195u, 11184810u, 15856113u, 14540253u,
		5855577u, 1315860u, 7105644u, 9605778u, 5526612u, 13684944u, 7895160u, 7368816u, 14935011u, 4802889u,
		8421504u, 5263440u, 10987431u, 16185078u, 7829367u, 9671571u, 8816262u, 8618883u, 2763306u, 13092807u,
		5987163u, 15329769u, 15658734u, 9408399u, 65793u, 4013373u
	};

	private static readonly uint[] SBOX3_3033 = new uint[256]
	{
		939538488u, 1090535745u, 369104406u, 1979741814u, 3640711641u, 2466288531u, 1610637408u, 4060148466u, 1912631922u, 3254829762u,
		2868947883u, 2583730842u, 1962964341u, 100664838u, 1459640151u, 2684395680u, 2432733585u, 4144035831u, 3036722613u, 3372272073u,
		2717950626u, 2348846220u, 3523269330u, 2415956112u, 4127258358u, 117442311u, 2801837991u, 654321447u, 2382401166u, 2986390194u,
		1224755529u, 3724599006u, 1124090691u, 1543527516u, 3607156695u, 3338717127u, 1040203326u, 4110480885u, 2399178639u, 1728079719u,
		520101663u, 402659352u, 1845522030u, 2936057775u, 788541231u, 3791708898u, 2231403909u, 218107149u, 1392530259u, 4026593520u,
		2617285788u, 1694524773u, 3925928682u, 2734728099u, 2919280302u, 2650840734u, 3959483628u, 2147516544u, 754986285u, 1795189611u,
		2818615464u, 721431339u, 905983542u, 2785060518u, 3305162181u, 2248181382u, 1291865421u, 855651123u, 4244700669u, 1711302246u,
		1476417624u, 2516620950u, 973093434u, 150997257u, 2499843477u, 268439568u, 2013296760u, 3623934168u, 1107313218u, 3422604492u,
		4009816047u, 637543974u, 3842041317u, 1627414881u, 436214298u, 1056980799u, 989870907u, 2181071490u, 3053500086u, 3674266587u,
		3556824276u, 2550175896u, 3892373736u, 2332068747u, 33554946u, 3942706155u, 167774730u, 738208812u, 486546717u, 2952835248u,
		1862299503u, 2365623693u, 2281736328u, 234884622u, 419436825u, 2264958855u, 1308642894u, 184552203u, 2835392937u, 201329676u,
		2030074233u, 285217041u, 2130739071u, 570434082u, 3875596263u, 1493195097u, 3774931425u, 3657489114u, 1023425853u, 3355494600u,
		301994514u, 67109892u, 1946186868u, 1409307732u, 805318704u, 2113961598u, 3019945140u, 671098920u, 1426085205u, 1744857192u,
		1342197840u, 3187719870u, 3489714384u, 3288384708u, 822096177u, 3405827019u, 704653866u, 2902502829u, 251662095u, 3389049546u,
		1879076976u, 4278255615u, 838873650u, 1761634665u, 134219784u, 1644192354u, 0u, 603989028u, 3506491857u, 4211145723u,
		3120609978u, 3976261101u, 1157645637u, 2164294017u, 1929409395u, 1828744557u, 2214626436u, 2667618207u, 3993038574u, 1241533002u,
		3271607235u, 771763758u, 3238052289u, 16777473u, 3858818790u, 620766501u, 1207978056u, 2566953369u, 3103832505u, 3003167667u,
		2063629179u, 4177590777u, 3456159438u, 3204497343u, 3741376479u, 1895854449u, 687876393u, 3439381965u, 1811967084u, 318771987u,
		1677747300u, 2600508315u, 1660969827u, 2634063261u, 3221274816u, 1258310475u, 3070277559u, 2768283045u, 2298513801u, 1593859935u,
		2969612721u, 385881879u, 4093703412u, 3154164924u, 3540046803u, 1174423110u, 3472936911u, 922761015u, 1577082462u, 1191200583u,
		2483066004u, 4194368250u, 4227923196u, 1526750043u, 2533398423u, 4261478142u, 1509972570u, 2885725356u, 1006648380u, 1275087948u,
		50332419u, 889206069u, 4076925939u, 587211555u, 3087055032u, 1560304989u, 1778412138u, 2449511058u, 3573601749u, 553656609u,
		1140868164u, 1358975313u, 3321939654u, 2097184125u, 956315961u, 2197848963u, 3691044060u, 2852170410u, 2080406652u, 1996519287u,
		1442862678u, 83887365u, 452991771u, 2751505572u, 352326933u, 872428596u, 503324190u, 469769244u, 4160813304u, 1375752786u,
		536879136u, 335549460u, 3909151209u, 3170942397u, 3707821533u, 3825263844u, 2701173153u, 3758153952u, 2315291274u, 4043370993u,
		3590379222u, 2046851706u, 3137387451u, 3808486371u, 1073758272u, 1325420367u
	};

	public virtual string AlgorithmName => "Camellia";

	public virtual bool IsPartialBlockOkay => false;

	private static uint rightRotate(uint x, int s)
	{
		return (x >> s) + (x << 32 - s);
	}

	private static uint leftRotate(uint x, int s)
	{
		return (x << s) + (x >> 32 - s);
	}

	private static void roldq(int rot, uint[] ki, int ioff, uint[] ko, int ooff)
	{
		ko[ooff] = (ki[ioff] << rot) | (ki[1 + ioff] >> 32 - rot);
		ko[1 + ooff] = (ki[1 + ioff] << rot) | (ki[2 + ioff] >> 32 - rot);
		ko[2 + ooff] = (ki[2 + ioff] << rot) | (ki[3 + ioff] >> 32 - rot);
		ko[3 + ooff] = (ki[3 + ioff] << rot) | (ki[ioff] >> 32 - rot);
		ki[ioff] = ko[ooff];
		ki[1 + ioff] = ko[1 + ooff];
		ki[2 + ioff] = ko[2 + ooff];
		ki[3 + ioff] = ko[3 + ooff];
	}

	private static void decroldq(int rot, uint[] ki, int ioff, uint[] ko, int ooff)
	{
		ko[2 + ooff] = (ki[ioff] << rot) | (ki[1 + ioff] >> 32 - rot);
		ko[3 + ooff] = (ki[1 + ioff] << rot) | (ki[2 + ioff] >> 32 - rot);
		ko[ooff] = (ki[2 + ioff] << rot) | (ki[3 + ioff] >> 32 - rot);
		ko[1 + ooff] = (ki[3 + ioff] << rot) | (ki[ioff] >> 32 - rot);
		ki[ioff] = ko[2 + ooff];
		ki[1 + ioff] = ko[3 + ooff];
		ki[2 + ioff] = ko[ooff];
		ki[3 + ioff] = ko[1 + ooff];
	}

	private static void roldqo32(int rot, uint[] ki, int ioff, uint[] ko, int ooff)
	{
		ko[ooff] = (ki[1 + ioff] << rot - 32) | (ki[2 + ioff] >> 64 - rot);
		ko[1 + ooff] = (ki[2 + ioff] << rot - 32) | (ki[3 + ioff] >> 64 - rot);
		ko[2 + ooff] = (ki[3 + ioff] << rot - 32) | (ki[ioff] >> 64 - rot);
		ko[3 + ooff] = (ki[ioff] << rot - 32) | (ki[1 + ioff] >> 64 - rot);
		ki[ioff] = ko[ooff];
		ki[1 + ioff] = ko[1 + ooff];
		ki[2 + ioff] = ko[2 + ooff];
		ki[3 + ioff] = ko[3 + ooff];
	}

	private static void decroldqo32(int rot, uint[] ki, int ioff, uint[] ko, int ooff)
	{
		ko[2 + ooff] = (ki[1 + ioff] << rot - 32) | (ki[2 + ioff] >> 64 - rot);
		ko[3 + ooff] = (ki[2 + ioff] << rot - 32) | (ki[3 + ioff] >> 64 - rot);
		ko[ooff] = (ki[3 + ioff] << rot - 32) | (ki[ioff] >> 64 - rot);
		ko[1 + ooff] = (ki[ioff] << rot - 32) | (ki[1 + ioff] >> 64 - rot);
		ki[ioff] = ko[2 + ooff];
		ki[1 + ioff] = ko[3 + ooff];
		ki[2 + ioff] = ko[ooff];
		ki[3 + ioff] = ko[1 + ooff];
	}

	private static uint bytes2uint(byte[] src, int offset)
	{
		uint num = 0u;
		for (int i = 0; i < 4; i++)
		{
			num = (num << 8) + src[i + offset];
		}
		return num;
	}

	private static void uint2bytes(uint word, byte[] dst, int offset)
	{
		for (int i = 0; i < 4; i++)
		{
			dst[3 - i + offset] = (byte)word;
			word >>= 8;
		}
	}

	private static void camelliaF2(uint[] s, uint[] skey, int keyoff)
	{
		uint num = s[0] ^ skey[keyoff];
		uint num2 = SBOX4_4404[(byte)num];
		num2 ^= SBOX3_3033[(byte)(num >> 8)];
		num2 ^= SBOX2_0222[(byte)(num >> 16)];
		num2 ^= SBOX1_1110[(byte)(num >> 24)];
		uint num3 = s[1] ^ skey[1 + keyoff];
		uint num4 = SBOX1_1110[(byte)num3];
		num4 ^= SBOX4_4404[(byte)(num3 >> 8)];
		num4 ^= SBOX3_3033[(byte)(num3 >> 16)];
		num4 ^= SBOX2_0222[(byte)(num3 >> 24)];
		uint[] array;
		(array = s)[2] = array[2] ^ (num2 ^ num4);
		(array = s)[3] = array[3] ^ (num2 ^ num4 ^ rightRotate(num2, 8));
		num = s[2] ^ skey[2 + keyoff];
		num2 = SBOX4_4404[(byte)num];
		num2 ^= SBOX3_3033[(byte)(num >> 8)];
		num2 ^= SBOX2_0222[(byte)(num >> 16)];
		num2 ^= SBOX1_1110[(byte)(num >> 24)];
		num3 = s[3] ^ skey[3 + keyoff];
		num4 = SBOX1_1110[(byte)num3];
		num4 ^= SBOX4_4404[(byte)(num3 >> 8)];
		num4 ^= SBOX3_3033[(byte)(num3 >> 16)];
		num4 ^= SBOX2_0222[(byte)(num3 >> 24)];
		(array = s)[0] = array[0] ^ (num2 ^ num4);
		(array = s)[1] = array[1] ^ (num2 ^ num4 ^ rightRotate(num2, 8));
	}

	private static void camelliaFLs(uint[] s, uint[] fkey, int keyoff)
	{
		uint[] array;
		(array = s)[1] = array[1] ^ leftRotate(s[0] & fkey[keyoff], 1);
		(array = s)[0] = array[0] ^ (fkey[1 + keyoff] | s[1]);
		(array = s)[2] = array[2] ^ (fkey[3 + keyoff] | s[3]);
		(array = s)[3] = array[3] ^ leftRotate(fkey[2 + keyoff] & s[2], 1);
	}

	private void setKey(bool forEncryption, byte[] key)
	{
		uint[] array = new uint[8];
		uint[] array2 = new uint[4];
		uint[] array3 = new uint[4];
		uint[] array4 = new uint[4];
		switch (key.Length)
		{
		case 16:
			_keyIs128 = true;
			array[0] = bytes2uint(key, 0);
			array[1] = bytes2uint(key, 4);
			array[2] = bytes2uint(key, 8);
			array[3] = bytes2uint(key, 12);
			array[4] = (array[5] = (array[6] = (array[7] = 0u)));
			break;
		case 24:
			array[0] = bytes2uint(key, 0);
			array[1] = bytes2uint(key, 4);
			array[2] = bytes2uint(key, 8);
			array[3] = bytes2uint(key, 12);
			array[4] = bytes2uint(key, 16);
			array[5] = bytes2uint(key, 20);
			array[6] = ~array[4];
			array[7] = ~array[5];
			_keyIs128 = false;
			break;
		case 32:
			array[0] = bytes2uint(key, 0);
			array[1] = bytes2uint(key, 4);
			array[2] = bytes2uint(key, 8);
			array[3] = bytes2uint(key, 12);
			array[4] = bytes2uint(key, 16);
			array[5] = bytes2uint(key, 20);
			array[6] = bytes2uint(key, 24);
			array[7] = bytes2uint(key, 28);
			_keyIs128 = false;
			break;
		default:
			throw new ArgumentException("key sizes are only 16/24/32 bytes.");
		}
		for (int i = 0; i < 4; i++)
		{
			array2[i] = array[i] ^ array[i + 4];
		}
		camelliaF2(array2, SIGMA, 0);
		for (int j = 0; j < 4; j++)
		{
			uint[] array5;
			uint[] array6 = (array5 = array2);
			int num = j;
			nint num2 = num;
			array6[num] = array5[num2] ^ array[j];
		}
		camelliaF2(array2, SIGMA, 4);
		if (_keyIs128)
		{
			if (forEncryption)
			{
				kw[0] = array[0];
				kw[1] = array[1];
				kw[2] = array[2];
				kw[3] = array[3];
				roldq(15, array, 0, subkey, 4);
				roldq(30, array, 0, subkey, 12);
				roldq(15, array, 0, array4, 0);
				subkey[18] = array4[2];
				subkey[19] = array4[3];
				roldq(17, array, 0, ke, 4);
				roldq(17, array, 0, subkey, 24);
				roldq(17, array, 0, subkey, 32);
				subkey[0] = array2[0];
				subkey[1] = array2[1];
				subkey[2] = array2[2];
				subkey[3] = array2[3];
				roldq(15, array2, 0, subkey, 8);
				roldq(15, array2, 0, ke, 0);
				roldq(15, array2, 0, array4, 0);
				subkey[16] = array4[0];
				subkey[17] = array4[1];
				roldq(15, array2, 0, subkey, 20);
				roldqo32(34, array2, 0, subkey, 28);
				roldq(17, array2, 0, kw, 4);
			}
			else
			{
				kw[4] = array[0];
				kw[5] = array[1];
				kw[6] = array[2];
				kw[7] = array[3];
				decroldq(15, array, 0, subkey, 28);
				decroldq(30, array, 0, subkey, 20);
				decroldq(15, array, 0, array4, 0);
				subkey[16] = array4[0];
				subkey[17] = array4[1];
				decroldq(17, array, 0, ke, 0);
				decroldq(17, array, 0, subkey, 8);
				decroldq(17, array, 0, subkey, 0);
				subkey[34] = array2[0];
				subkey[35] = array2[1];
				subkey[32] = array2[2];
				subkey[33] = array2[3];
				decroldq(15, array2, 0, subkey, 24);
				decroldq(15, array2, 0, ke, 4);
				decroldq(15, array2, 0, array4, 0);
				subkey[18] = array4[2];
				subkey[19] = array4[3];
				decroldq(15, array2, 0, subkey, 12);
				decroldqo32(34, array2, 0, subkey, 4);
				roldq(17, array2, 0, kw, 0);
			}
			return;
		}
		for (int k = 0; k < 4; k++)
		{
			array3[k] = array2[k] ^ array[k + 4];
		}
		camelliaF2(array3, SIGMA, 8);
		if (forEncryption)
		{
			kw[0] = array[0];
			kw[1] = array[1];
			kw[2] = array[2];
			kw[3] = array[3];
			roldqo32(45, array, 0, subkey, 16);
			roldq(15, array, 0, ke, 4);
			roldq(17, array, 0, subkey, 32);
			roldqo32(34, array, 0, subkey, 44);
			roldq(15, array, 4, subkey, 4);
			roldq(15, array, 4, ke, 0);
			roldq(30, array, 4, subkey, 24);
			roldqo32(34, array, 4, subkey, 36);
			roldq(15, array2, 0, subkey, 8);
			roldq(30, array2, 0, subkey, 20);
			ke[8] = array2[1];
			ke[9] = array2[2];
			ke[10] = array2[3];
			ke[11] = array2[0];
			roldqo32(49, array2, 0, subkey, 40);
			subkey[0] = array3[0];
			subkey[1] = array3[1];
			subkey[2] = array3[2];
			subkey[3] = array3[3];
			roldq(30, array3, 0, subkey, 12);
			roldq(30, array3, 0, subkey, 28);
			roldqo32(51, array3, 0, kw, 4);
		}
		else
		{
			kw[4] = array[0];
			kw[5] = array[1];
			kw[6] = array[2];
			kw[7] = array[3];
			decroldqo32(45, array, 0, subkey, 28);
			decroldq(15, array, 0, ke, 4);
			decroldq(17, array, 0, subkey, 12);
			decroldqo32(34, array, 0, subkey, 0);
			decroldq(15, array, 4, subkey, 40);
			decroldq(15, array, 4, ke, 8);
			decroldq(30, array, 4, subkey, 20);
			decroldqo32(34, array, 4, subkey, 8);
			decroldq(15, array2, 0, subkey, 36);
			decroldq(30, array2, 0, subkey, 24);
			ke[2] = array2[1];
			ke[3] = array2[2];
			ke[0] = array2[3];
			ke[1] = array2[0];
			decroldqo32(49, array2, 0, subkey, 4);
			subkey[46] = array3[0];
			subkey[47] = array3[1];
			subkey[44] = array3[2];
			subkey[45] = array3[3];
			decroldq(30, array3, 0, subkey, 32);
			decroldq(30, array3, 0, subkey, 16);
			roldqo32(51, array3, 0, kw, 0);
		}
	}

	private int processBlock128(byte[] input, int inOff, byte[] output, int outOff)
	{
		uint[] array;
		for (int i = 0; i < 4; i++)
		{
			state[i] = bytes2uint(input, inOff + i * 4);
			uint[] array2 = (array = state);
			int num = i;
			nint num2 = num;
			array2[num] = array[num2] ^ kw[i];
		}
		camelliaF2(state, subkey, 0);
		camelliaF2(state, subkey, 4);
		camelliaF2(state, subkey, 8);
		camelliaFLs(state, ke, 0);
		camelliaF2(state, subkey, 12);
		camelliaF2(state, subkey, 16);
		camelliaF2(state, subkey, 20);
		camelliaFLs(state, ke, 4);
		camelliaF2(state, subkey, 24);
		camelliaF2(state, subkey, 28);
		camelliaF2(state, subkey, 32);
		(array = state)[2] = array[2] ^ kw[4];
		(array = state)[3] = array[3] ^ kw[5];
		(array = state)[0] = array[0] ^ kw[6];
		(array = state)[1] = array[1] ^ kw[7];
		uint2bytes(state[2], output, outOff);
		uint2bytes(state[3], output, outOff + 4);
		uint2bytes(state[0], output, outOff + 8);
		uint2bytes(state[1], output, outOff + 12);
		return 16;
	}

	private int processBlock192or256(byte[] input, int inOff, byte[] output, int outOff)
	{
		uint[] array;
		for (int i = 0; i < 4; i++)
		{
			state[i] = bytes2uint(input, inOff + i * 4);
			uint[] array2 = (array = state);
			int num = i;
			nint num2 = num;
			array2[num] = array[num2] ^ kw[i];
		}
		camelliaF2(state, subkey, 0);
		camelliaF2(state, subkey, 4);
		camelliaF2(state, subkey, 8);
		camelliaFLs(state, ke, 0);
		camelliaF2(state, subkey, 12);
		camelliaF2(state, subkey, 16);
		camelliaF2(state, subkey, 20);
		camelliaFLs(state, ke, 4);
		camelliaF2(state, subkey, 24);
		camelliaF2(state, subkey, 28);
		camelliaF2(state, subkey, 32);
		camelliaFLs(state, ke, 8);
		camelliaF2(state, subkey, 36);
		camelliaF2(state, subkey, 40);
		camelliaF2(state, subkey, 44);
		(array = state)[2] = array[2] ^ kw[4];
		(array = state)[3] = array[3] ^ kw[5];
		(array = state)[0] = array[0] ^ kw[6];
		(array = state)[1] = array[1] ^ kw[7];
		uint2bytes(state[2], output, outOff);
		uint2bytes(state[3], output, outOff + 4);
		uint2bytes(state[0], output, outOff + 8);
		uint2bytes(state[1], output, outOff + 12);
		return 16;
	}

	public virtual void Init(bool forEncryption, ICipherParameters parameters)
	{
		if (!(parameters is KeyParameter))
		{
			throw new ArgumentException("only simple KeyParameter expected.");
		}
		setKey(forEncryption, ((KeyParameter)parameters).GetKey());
		initialised = true;
	}

	public virtual int GetBlockSize()
	{
		return 16;
	}

	public virtual int ProcessBlock(byte[] input, int inOff, byte[] output, int outOff)
	{
		if (!initialised)
		{
			throw new InvalidOperationException("Camellia engine not initialised");
		}
		Check.DataLength(input, inOff, 16, "input buffer too short");
		Check.OutputLength(output, outOff, 16, "output buffer too short");
		if (_keyIs128)
		{
			return processBlock128(input, inOff, output, outOff);
		}
		return processBlock192or256(input, inOff, output, outOff);
	}

	public virtual void Reset()
	{
	}
}
