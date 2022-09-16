using System;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Utilities;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Engines;

public class AesEngine : IBlockCipher
{
	private const uint m1 = 2155905152u;

	private const uint m2 = 2139062143u;

	private const uint m3 = 27u;

	private const uint m4 = 3233857728u;

	private const uint m5 = 1061109567u;

	private const int BLOCK_SIZE = 16;

	private static readonly byte[] S = new byte[256]
	{
		99, 124, 119, 123, 242, 107, 111, 197, 48, 1,
		103, 43, 254, 215, 171, 118, 202, 130, 201, 125,
		250, 89, 71, 240, 173, 212, 162, 175, 156, 164,
		114, 192, 183, 253, 147, 38, 54, 63, 247, 204,
		52, 165, 229, 241, 113, 216, 49, 21, 4, 199,
		35, 195, 24, 150, 5, 154, 7, 18, 128, 226,
		235, 39, 178, 117, 9, 131, 44, 26, 27, 110,
		90, 160, 82, 59, 214, 179, 41, 227, 47, 132,
		83, 209, 0, 237, 32, 252, 177, 91, 106, 203,
		190, 57, 74, 76, 88, 207, 208, 239, 170, 251,
		67, 77, 51, 133, 69, 249, 2, 127, 80, 60,
		159, 168, 81, 163, 64, 143, 146, 157, 56, 245,
		188, 182, 218, 33, 16, 255, 243, 210, 205, 12,
		19, 236, 95, 151, 68, 23, 196, 167, 126, 61,
		100, 93, 25, 115, 96, 129, 79, 220, 34, 42,
		144, 136, 70, 238, 184, 20, 222, 94, 11, 219,
		224, 50, 58, 10, 73, 6, 36, 92, 194, 211,
		172, 98, 145, 149, 228, 121, 231, 200, 55, 109,
		141, 213, 78, 169, 108, 86, 244, 234, 101, 122,
		174, 8, 186, 120, 37, 46, 28, 166, 180, 198,
		232, 221, 116, 31, 75, 189, 139, 138, 112, 62,
		181, 102, 72, 3, 246, 14, 97, 53, 87, 185,
		134, 193, 29, 158, 225, 248, 152, 17, 105, 217,
		142, 148, 155, 30, 135, 233, 206, 85, 40, 223,
		140, 161, 137, 13, 191, 230, 66, 104, 65, 153,
		45, 15, 176, 84, 187, 22
	};

	private static readonly byte[] Si = new byte[256]
	{
		82, 9, 106, 213, 48, 54, 165, 56, 191, 64,
		163, 158, 129, 243, 215, 251, 124, 227, 57, 130,
		155, 47, 255, 135, 52, 142, 67, 68, 196, 222,
		233, 203, 84, 123, 148, 50, 166, 194, 35, 61,
		238, 76, 149, 11, 66, 250, 195, 78, 8, 46,
		161, 102, 40, 217, 36, 178, 118, 91, 162, 73,
		109, 139, 209, 37, 114, 248, 246, 100, 134, 104,
		152, 22, 212, 164, 92, 204, 93, 101, 182, 146,
		108, 112, 72, 80, 253, 237, 185, 218, 94, 21,
		70, 87, 167, 141, 157, 132, 144, 216, 171, 0,
		140, 188, 211, 10, 247, 228, 88, 5, 184, 179,
		69, 6, 208, 44, 30, 143, 202, 63, 15, 2,
		193, 175, 189, 3, 1, 19, 138, 107, 58, 145,
		17, 65, 79, 103, 220, 234, 151, 242, 207, 206,
		240, 180, 230, 115, 150, 172, 116, 34, 231, 173,
		53, 133, 226, 249, 55, 232, 28, 117, 223, 110,
		71, 241, 26, 113, 29, 41, 197, 137, 111, 183,
		98, 14, 170, 24, 190, 27, 252, 86, 62, 75,
		198, 210, 121, 32, 154, 219, 192, 254, 120, 205,
		90, 244, 31, 221, 168, 51, 136, 7, 199, 49,
		177, 18, 16, 89, 39, 128, 236, 95, 96, 81,
		127, 169, 25, 181, 74, 13, 45, 229, 122, 159,
		147, 201, 156, 239, 160, 224, 59, 77, 174, 42,
		245, 176, 200, 235, 187, 60, 131, 83, 153, 97,
		23, 43, 4, 126, 186, 119, 214, 38, 225, 105,
		20, 99, 85, 33, 12, 125
	};

	private static readonly byte[] rcon = new byte[30]
	{
		1, 2, 4, 8, 16, 32, 64, 128, 27, 54,
		108, 216, 171, 77, 154, 47, 94, 188, 99, 198,
		151, 53, 106, 212, 179, 125, 250, 239, 197, 145
	};

	private static readonly uint[] T0 = new uint[256]
	{
		2774754246u, 2222750968u, 2574743534u, 2373680118u, 234025727u, 3177933782u, 2976870366u, 1422247313u, 1345335392u, 50397442u,
		2842126286u, 2099981142u, 436141799u, 1658312629u, 3870010189u, 2591454956u, 1170918031u, 2642575903u, 1086966153u, 2273148410u,
		368769775u, 3948501426u, 3376891790u, 200339707u, 3970805057u, 1742001331u, 4255294047u, 3937382213u, 3214711843u, 4154762323u,
		2524082916u, 1539358875u, 3266819957u, 486407649u, 2928907069u, 1780885068u, 1513502316u, 1094664062u, 49805301u, 1338821763u,
		1546925160u, 4104496465u, 887481809u, 150073849u, 2473685474u, 1943591083u, 1395732834u, 1058346282u, 201589768u, 1388824469u,
		1696801606u, 1589887901u, 672667696u, 2711000631u, 251987210u, 3046808111u, 151455502u, 907153956u, 2608889883u, 1038279391u,
		652995533u, 1764173646u, 3451040383u, 2675275242u, 453576978u, 2659418909u, 1949051992u, 773462580u, 756751158u, 2993581788u,
		3998898868u, 4221608027u, 4132590244u, 1295727478u, 1641469623u, 3467883389u, 2066295122u, 1055122397u, 1898917726u, 2542044179u,
		4115878822u, 1758581177u, 0u, 753790401u, 1612718144u, 536673507u, 3367088505u, 3982187446u, 3194645204u, 1187761037u,
		3653156455u, 1262041458u, 3729410708u, 3561770136u, 3898103984u, 1255133061u, 1808847035u, 720367557u, 3853167183u, 385612781u,
		3309519750u, 3612167578u, 1429418854u, 2491778321u, 3477423498u, 284817897u, 100794884u, 2172616702u, 4031795360u, 1144798328u,
		3131023141u, 3819481163u, 4082192802u, 4272137053u, 3225436288u, 2324664069u, 2912064063u, 3164445985u, 1211644016u, 83228145u,
		3753688163u, 3249976951u, 1977277103u, 1663115586u, 806359072u, 452984805u, 250868733u, 1842533055u, 1288555905u, 336333848u,
		890442534u, 804056259u, 3781124030u, 2727843637u, 3427026056u, 957814574u, 1472513171u, 4071073621u, 2189328124u, 1195195770u,
		2892260552u, 3881655738u, 723065138u, 2507371494u, 2690670784u, 2558624025u, 3511635870u, 2145180835u, 1713513028u, 2116692564u,
		2878378043u, 2206763019u, 3393603212u, 703524551u, 3552098411u, 1007948840u, 2044649127u, 3797835452u, 487262998u, 1994120109u,
		1004593371u, 1446130276u, 1312438900u, 503974420u, 3679013266u, 168166924u, 1814307912u, 3831258296u, 1573044895u, 1859376061u,
		4021070915u, 2791465668u, 2828112185u, 2761266481u, 937747667u, 2339994098u, 854058965u, 1137232011u, 1496790894u, 3077402074u,
		2358086913u, 1691735473u, 3528347292u, 3769215305u, 3027004632u, 4199962284u, 133494003u, 636152527u, 2942657994u, 2390391540u,
		3920539207u, 403179536u, 3585784431u, 2289596656u, 1864705354u, 1915629148u, 605822008u, 4054230615u, 3350508659u, 1371981463u,
		602466507u, 2094914977u, 2624877800u, 555687742u, 3712699286u, 3703422305u, 2257292045u, 2240449039u, 2423288032u, 1111375484u,
		3300242801u, 2858837708u, 3628615824u, 84083462u, 32962295u, 302911004u, 2741068226u, 1597322602u, 4183250862u, 3501832553u,
		2441512471u, 1489093017u, 656219450u, 3114180135u, 954327513u, 335083755u, 3013122091u, 856756514u, 3144247762u, 1893325225u,
		2307821063u, 2811532339u, 3063651117u, 572399164u, 2458355477u, 552200649u, 1238290055u, 4283782570u, 2015897680u, 2061492133u,
		2408352771u, 4171342169u, 2156497161u, 386731290u, 3669999461u, 837215959u, 3326231172u, 3093850320u, 3275833730u, 2962856233u,
		1999449434u, 286199582u, 3417354363u, 4233385128u, 3602627437u, 974525996u
	};

	private static readonly uint[] Tinv0 = new uint[256]
	{
		1353184337u, 1399144830u, 3282310938u, 2522752826u, 3412831035u, 4047871263u, 2874735276u, 2466505547u, 1442459680u, 4134368941u,
		2440481928u, 625738485u, 4242007375u, 3620416197u, 2151953702u, 2409849525u, 1230680542u, 1729870373u, 2551114309u, 3787521629u,
		41234371u, 317738113u, 2744600205u, 3338261355u, 3881799427u, 2510066197u, 3950669247u, 3663286933u, 763608788u, 3542185048u,
		694804553u, 1154009486u, 1787413109u, 2021232372u, 1799248025u, 3715217703u, 3058688446u, 397248752u, 1722556617u, 3023752829u,
		407560035u, 2184256229u, 1613975959u, 1165972322u, 3765920945u, 2226023355u, 480281086u, 2485848313u, 1483229296u, 436028815u,
		2272059028u, 3086515026u, 601060267u, 3791801202u, 1468997603u, 715871590u, 120122290u, 63092015u, 2591802758u, 2768779219u,
		4068943920u, 2997206819u, 3127509762u, 1552029421u, 723308426u, 2461301159u, 4042393587u, 2715969870u, 3455375973u, 3586000134u,
		526529745u, 2331944644u, 2639474228u, 2689987490u, 853641733u, 1978398372u, 971801355u, 2867814464u, 111112542u, 1360031421u,
		4186579262u, 1023860118u, 2919579357u, 1186850381u, 3045938321u, 90031217u, 1876166148u, 4279586912u, 620468249u, 2548678102u,
		3426959497u, 2006899047u, 3175278768u, 2290845959u, 945494503u, 3689859193u, 1191869601u, 3910091388u, 3374220536u, 0u,
		2206629897u, 1223502642u, 2893025566u, 1316117100u, 4227796733u, 1446544655u, 517320253u, 658058550u, 1691946762u, 564550760u,
		3511966619u, 976107044u, 2976320012u, 266819475u, 3533106868u, 2660342555u, 1338359936u, 2720062561u, 1766553434u, 370807324u,
		179999714u, 3844776128u, 1138762300u, 488053522u, 185403662u, 2915535858u, 3114841645u, 3366526484u, 2233069911u, 1275557295u,
		3151862254u, 4250959779u, 2670068215u, 3170202204u, 3309004356u, 880737115u, 1982415755u, 3703972811u, 1761406390u, 1676797112u,
		3403428311u, 277177154u, 1076008723u, 538035844u, 2099530373u, 4164795346u, 288553390u, 1839278535u, 1261411869u, 4080055004u,
		3964831245u, 3504587127u, 1813426987u, 2579067049u, 4199060497u, 577038663u, 3297574056u, 440397984u, 3626794326u, 4019204898u,
		3343796615u, 3251714265u, 4272081548u, 906744984u, 3481400742u, 685669029u, 646887386u, 2764025151u, 3835509292u, 227702864u,
		2613862250u, 1648787028u, 3256061430u, 3904428176u, 1593260334u, 4121936770u, 3196083615u, 2090061929u, 2838353263u, 3004310991u,
		999926984u, 2809993232u, 1852021992u, 2075868123u, 158869197u, 4095236462u, 28809964u, 2828685187u, 1701746150u, 2129067946u,
		147831841u, 3873969647u, 3650873274u, 3459673930u, 3557400554u, 3598495785u, 2947720241u, 824393514u, 815048134u, 3227951669u,
		935087732u, 2798289660u, 2966458592u, 366520115u, 1251476721u, 4158319681u, 240176511u, 804688151u, 2379631990u, 1303441219u,
		1414376140u, 3741619940u, 3820343710u, 461924940u, 3089050817u, 2136040774u, 82468509u, 1563790337u, 1937016826u, 776014843u,
		1511876531u, 1389550482u, 861278441u, 323475053u, 2355222426u, 2047648055u, 2383738969u, 2302415851u, 3995576782u, 902390199u,
		3991215329u, 1018251130u, 1507840668u, 1064563285u, 2043548696u, 3208103795u, 3939366739u, 1537932639u, 342834655u, 2262516856u,
		2180231114u, 1053059257u, 741614648u, 1598071746u, 1925389590u, 203809468u, 2336832552u, 1100287487u, 1895934009u, 3736275976u,
		2632234200u, 2428589668u, 1636092795u, 1890988757u, 1952214088u, 1113045200u
	};

	private int ROUNDS;

	private uint[][] WorkingKey;

	private uint C0;

	private uint C1;

	private uint C2;

	private uint C3;

	private bool forEncryption;

	private byte[] s;

	public virtual string AlgorithmName => "AES";

	public virtual bool IsPartialBlockOkay => false;

	private static uint Shift(uint r, int shift)
	{
		return (r >> shift) | (r << 32 - shift);
	}

	private static uint FFmulX(uint x)
	{
		return ((x & 0x7F7F7F7F) << 1) ^ (((x & 0x80808080u) >> 7) * 27);
	}

	private static uint FFmulX2(uint x)
	{
		uint num = (x & 0x3F3F3F3F) << 2;
		uint num2 = x & 0xC0C0C0C0u;
		num2 ^= num2 >> 1;
		return num ^ (num2 >> 2) ^ (num2 >> 5);
	}

	private static uint Inv_Mcol(uint x)
	{
		uint num = x;
		uint num2 = num ^ Shift(num, 8);
		num ^= FFmulX(num2);
		num2 ^= FFmulX2(num);
		return num ^ (num2 ^ Shift(num2, 16));
	}

	private static uint SubWord(uint x)
	{
		return (uint)(S[x & 0xFF] | (S[(x >> 8) & 0xFF] << 8) | (S[(x >> 16) & 0xFF] << 16) | (S[(x >> 24) & 0xFF] << 24));
	}

	private uint[][] GenerateWorkingKey(byte[] key, bool forEncryption)
	{
		int num = key.Length;
		if (num < 16 || num > 32 || ((uint)num & 7u) != 0)
		{
			throw new ArgumentException("Key length not 128/192/256 bits.");
		}
		int num2 = num >> 2;
		ROUNDS = num2 + 6;
		uint[][] array = new uint[ROUNDS + 1][];
		for (int i = 0; i <= ROUNDS; i++)
		{
			array[i] = new uint[4];
		}
		switch (num2)
		{
		case 4:
		{
			uint num21 = Pack.LE_To_UInt32(key, 0);
			array[0][0] = num21;
			uint num22 = Pack.LE_To_UInt32(key, 4);
			array[0][1] = num22;
			uint num23 = Pack.LE_To_UInt32(key, 8);
			array[0][2] = num23;
			uint num24 = Pack.LE_To_UInt32(key, 12);
			array[0][3] = num24;
			for (int l = 1; l <= 10; l++)
			{
				uint num25 = SubWord(Shift(num24, 8)) ^ rcon[l - 1];
				num21 ^= num25;
				array[l][0] = num21;
				num22 ^= num21;
				array[l][1] = num22;
				num23 ^= num22;
				array[l][2] = num23;
				num24 ^= num23;
				array[l][3] = num24;
			}
			break;
		}
		case 6:
		{
			uint num13 = Pack.LE_To_UInt32(key, 0);
			array[0][0] = num13;
			uint num14 = Pack.LE_To_UInt32(key, 4);
			array[0][1] = num14;
			uint num15 = Pack.LE_To_UInt32(key, 8);
			array[0][2] = num15;
			uint num16 = Pack.LE_To_UInt32(key, 12);
			array[0][3] = num16;
			uint num17 = Pack.LE_To_UInt32(key, 16);
			array[1][0] = num17;
			uint num18 = Pack.LE_To_UInt32(key, 20);
			array[1][1] = num18;
			uint num19 = 1u;
			uint num20 = SubWord(Shift(num18, 8)) ^ num19;
			num19 <<= 1;
			num13 ^= num20;
			array[1][2] = num13;
			num14 ^= num13;
			array[1][3] = num14;
			num15 ^= num14;
			array[2][0] = num15;
			num16 ^= num15;
			array[2][1] = num16;
			num17 ^= num16;
			array[2][2] = num17;
			num18 ^= num17;
			array[2][3] = num18;
			for (int k = 3; k < 12; k += 3)
			{
				num20 = SubWord(Shift(num18, 8)) ^ num19;
				num19 <<= 1;
				num13 ^= num20;
				array[k][0] = num13;
				num14 ^= num13;
				array[k][1] = num14;
				num15 ^= num14;
				array[k][2] = num15;
				num16 ^= num15;
				array[k][3] = num16;
				num17 ^= num16;
				array[k + 1][0] = num17;
				num18 ^= num17;
				array[k + 1][1] = num18;
				num20 = SubWord(Shift(num18, 8)) ^ num19;
				num19 <<= 1;
				num13 ^= num20;
				array[k + 1][2] = num13;
				num14 ^= num13;
				array[k + 1][3] = num14;
				num15 ^= num14;
				array[k + 2][0] = num15;
				num16 ^= num15;
				array[k + 2][1] = num16;
				num17 ^= num16;
				array[k + 2][2] = num17;
				num18 ^= num17;
				array[k + 2][3] = num18;
			}
			num20 = SubWord(Shift(num18, 8)) ^ num19;
			num13 ^= num20;
			array[12][0] = num13;
			num14 ^= num13;
			array[12][1] = num14;
			num15 ^= num14;
			array[12][2] = num15;
			num16 ^= num15;
			array[12][3] = num16;
			break;
		}
		case 8:
		{
			uint num3 = Pack.LE_To_UInt32(key, 0);
			array[0][0] = num3;
			uint num4 = Pack.LE_To_UInt32(key, 4);
			array[0][1] = num4;
			uint num5 = Pack.LE_To_UInt32(key, 8);
			array[0][2] = num5;
			uint num6 = Pack.LE_To_UInt32(key, 12);
			array[0][3] = num6;
			uint num7 = Pack.LE_To_UInt32(key, 16);
			array[1][0] = num7;
			uint num8 = Pack.LE_To_UInt32(key, 20);
			array[1][1] = num8;
			uint num9 = Pack.LE_To_UInt32(key, 24);
			array[1][2] = num9;
			uint num10 = Pack.LE_To_UInt32(key, 28);
			array[1][3] = num10;
			uint num11 = 1u;
			uint num12;
			for (int j = 2; j < 14; j += 2)
			{
				num12 = SubWord(Shift(num10, 8)) ^ num11;
				num11 <<= 1;
				num3 ^= num12;
				array[j][0] = num3;
				num4 ^= num3;
				array[j][1] = num4;
				num5 ^= num4;
				array[j][2] = num5;
				num6 ^= num5;
				array[j][3] = num6;
				num12 = SubWord(num6);
				num7 ^= num12;
				array[j + 1][0] = num7;
				num8 ^= num7;
				array[j + 1][1] = num8;
				num9 ^= num8;
				array[j + 1][2] = num9;
				num10 ^= num9;
				array[j + 1][3] = num10;
			}
			num12 = SubWord(Shift(num10, 8)) ^ num11;
			num3 ^= num12;
			array[14][0] = num3;
			num4 ^= num3;
			array[14][1] = num4;
			num5 ^= num4;
			array[14][2] = num5;
			num6 ^= num5;
			array[14][3] = num6;
			break;
		}
		default:
			throw new InvalidOperationException("Should never get here");
		}
		if (!forEncryption)
		{
			for (int m = 1; m < ROUNDS; m++)
			{
				uint[] array2 = array[m];
				for (int n = 0; n < 4; n++)
				{
					array2[n] = Inv_Mcol(array2[n]);
				}
			}
		}
		return array;
	}

	public virtual void Init(bool forEncryption, ICipherParameters parameters)
	{
		if (!(parameters is KeyParameter keyParameter))
		{
			throw new ArgumentException("invalid parameter passed to AES init - " + Platform.GetTypeName(parameters));
		}
		WorkingKey = GenerateWorkingKey(keyParameter.GetKey(), forEncryption);
		this.forEncryption = forEncryption;
		s = Arrays.Clone(forEncryption ? S : Si);
	}

	public virtual int GetBlockSize()
	{
		return 16;
	}

	public virtual int ProcessBlock(byte[] input, int inOff, byte[] output, int outOff)
	{
		if (WorkingKey == null)
		{
			throw new InvalidOperationException("AES engine not initialised");
		}
		Check.DataLength(input, inOff, 16, "input buffer too short");
		Check.OutputLength(output, outOff, 16, "output buffer too short");
		UnPackBlock(input, inOff);
		if (forEncryption)
		{
			EncryptBlock(WorkingKey);
		}
		else
		{
			DecryptBlock(WorkingKey);
		}
		PackBlock(output, outOff);
		return 16;
	}

	public virtual void Reset()
	{
	}

	private void UnPackBlock(byte[] bytes, int off)
	{
		C0 = Pack.LE_To_UInt32(bytes, off);
		C1 = Pack.LE_To_UInt32(bytes, off + 4);
		C2 = Pack.LE_To_UInt32(bytes, off + 8);
		C3 = Pack.LE_To_UInt32(bytes, off + 12);
	}

	private void PackBlock(byte[] bytes, int off)
	{
		Pack.UInt32_To_LE(C0, bytes, off);
		Pack.UInt32_To_LE(C1, bytes, off + 4);
		Pack.UInt32_To_LE(C2, bytes, off + 8);
		Pack.UInt32_To_LE(C3, bytes, off + 12);
	}

	private void EncryptBlock(uint[][] KW)
	{
		uint[] array = KW[0];
		uint num = C0 ^ array[0];
		uint num2 = C1 ^ array[1];
		uint num3 = C2 ^ array[2];
		uint num4 = C3 ^ array[3];
		int num5 = 1;
		uint num6;
		uint num7;
		uint num8;
		while (num5 < ROUNDS - 1)
		{
			array = KW[num5++];
			num6 = T0[num & 0xFF] ^ Shift(T0[(num2 >> 8) & 0xFF], 24) ^ Shift(T0[(num3 >> 16) & 0xFF], 16) ^ Shift(T0[(num4 >> 24) & 0xFF], 8) ^ array[0];
			num7 = T0[num2 & 0xFF] ^ Shift(T0[(num3 >> 8) & 0xFF], 24) ^ Shift(T0[(num4 >> 16) & 0xFF], 16) ^ Shift(T0[(num >> 24) & 0xFF], 8) ^ array[1];
			num8 = T0[num3 & 0xFF] ^ Shift(T0[(num4 >> 8) & 0xFF], 24) ^ Shift(T0[(num >> 16) & 0xFF], 16) ^ Shift(T0[(num2 >> 24) & 0xFF], 8) ^ array[2];
			num4 = T0[num4 & 0xFF] ^ Shift(T0[(num >> 8) & 0xFF], 24) ^ Shift(T0[(num2 >> 16) & 0xFF], 16) ^ Shift(T0[(num3 >> 24) & 0xFF], 8) ^ array[3];
			array = KW[num5++];
			num = T0[num6 & 0xFF] ^ Shift(T0[(num7 >> 8) & 0xFF], 24) ^ Shift(T0[(num8 >> 16) & 0xFF], 16) ^ Shift(T0[(num4 >> 24) & 0xFF], 8) ^ array[0];
			num2 = T0[num7 & 0xFF] ^ Shift(T0[(num8 >> 8) & 0xFF], 24) ^ Shift(T0[(num4 >> 16) & 0xFF], 16) ^ Shift(T0[(num6 >> 24) & 0xFF], 8) ^ array[1];
			num3 = T0[num8 & 0xFF] ^ Shift(T0[(num4 >> 8) & 0xFF], 24) ^ Shift(T0[(num6 >> 16) & 0xFF], 16) ^ Shift(T0[(num7 >> 24) & 0xFF], 8) ^ array[2];
			num4 = T0[num4 & 0xFF] ^ Shift(T0[(num6 >> 8) & 0xFF], 24) ^ Shift(T0[(num7 >> 16) & 0xFF], 16) ^ Shift(T0[(num8 >> 24) & 0xFF], 8) ^ array[3];
		}
		array = KW[num5++];
		num6 = T0[num & 0xFF] ^ Shift(T0[(num2 >> 8) & 0xFF], 24) ^ Shift(T0[(num3 >> 16) & 0xFF], 16) ^ Shift(T0[(num4 >> 24) & 0xFF], 8) ^ array[0];
		num7 = T0[num2 & 0xFF] ^ Shift(T0[(num3 >> 8) & 0xFF], 24) ^ Shift(T0[(num4 >> 16) & 0xFF], 16) ^ Shift(T0[(num >> 24) & 0xFF], 8) ^ array[1];
		num8 = T0[num3 & 0xFF] ^ Shift(T0[(num4 >> 8) & 0xFF], 24) ^ Shift(T0[(num >> 16) & 0xFF], 16) ^ Shift(T0[(num2 >> 24) & 0xFF], 8) ^ array[2];
		num4 = T0[num4 & 0xFF] ^ Shift(T0[(num >> 8) & 0xFF], 24) ^ Shift(T0[(num2 >> 16) & 0xFF], 16) ^ Shift(T0[(num3 >> 24) & 0xFF], 8) ^ array[3];
		array = KW[num5];
		C0 = (uint)(S[num6 & 0xFF] ^ (S[(num7 >> 8) & 0xFF] << 8) ^ (s[(num8 >> 16) & 0xFF] << 16) ^ (s[(num4 >> 24) & 0xFF] << 24)) ^ array[0];
		C1 = (uint)(s[num7 & 0xFF] ^ (S[(num8 >> 8) & 0xFF] << 8) ^ (S[(num4 >> 16) & 0xFF] << 16) ^ (s[(num6 >> 24) & 0xFF] << 24)) ^ array[1];
		C2 = (uint)(s[num8 & 0xFF] ^ (S[(num4 >> 8) & 0xFF] << 8) ^ (S[(num6 >> 16) & 0xFF] << 16) ^ (S[(num7 >> 24) & 0xFF] << 24)) ^ array[2];
		C3 = (uint)(s[num4 & 0xFF] ^ (s[(num6 >> 8) & 0xFF] << 8) ^ (s[(num7 >> 16) & 0xFF] << 16) ^ (S[(num8 >> 24) & 0xFF] << 24)) ^ array[3];
	}

	private void DecryptBlock(uint[][] KW)
	{
		uint[] array = KW[ROUNDS];
		uint num = C0 ^ array[0];
		uint num2 = C1 ^ array[1];
		uint num3 = C2 ^ array[2];
		uint num4 = C3 ^ array[3];
		int num5 = ROUNDS - 1;
		uint num6;
		uint num7;
		uint num8;
		while (num5 > 1)
		{
			array = KW[num5--];
			num6 = Tinv0[num & 0xFF] ^ Shift(Tinv0[(num4 >> 8) & 0xFF], 24) ^ Shift(Tinv0[(num3 >> 16) & 0xFF], 16) ^ Shift(Tinv0[(num2 >> 24) & 0xFF], 8) ^ array[0];
			num7 = Tinv0[num2 & 0xFF] ^ Shift(Tinv0[(num >> 8) & 0xFF], 24) ^ Shift(Tinv0[(num4 >> 16) & 0xFF], 16) ^ Shift(Tinv0[(num3 >> 24) & 0xFF], 8) ^ array[1];
			num8 = Tinv0[num3 & 0xFF] ^ Shift(Tinv0[(num2 >> 8) & 0xFF], 24) ^ Shift(Tinv0[(num >> 16) & 0xFF], 16) ^ Shift(Tinv0[(num4 >> 24) & 0xFF], 8) ^ array[2];
			num4 = Tinv0[num4 & 0xFF] ^ Shift(Tinv0[(num3 >> 8) & 0xFF], 24) ^ Shift(Tinv0[(num2 >> 16) & 0xFF], 16) ^ Shift(Tinv0[(num >> 24) & 0xFF], 8) ^ array[3];
			array = KW[num5--];
			num = Tinv0[num6 & 0xFF] ^ Shift(Tinv0[(num4 >> 8) & 0xFF], 24) ^ Shift(Tinv0[(num8 >> 16) & 0xFF], 16) ^ Shift(Tinv0[(num7 >> 24) & 0xFF], 8) ^ array[0];
			num2 = Tinv0[num7 & 0xFF] ^ Shift(Tinv0[(num6 >> 8) & 0xFF], 24) ^ Shift(Tinv0[(num4 >> 16) & 0xFF], 16) ^ Shift(Tinv0[(num8 >> 24) & 0xFF], 8) ^ array[1];
			num3 = Tinv0[num8 & 0xFF] ^ Shift(Tinv0[(num7 >> 8) & 0xFF], 24) ^ Shift(Tinv0[(num6 >> 16) & 0xFF], 16) ^ Shift(Tinv0[(num4 >> 24) & 0xFF], 8) ^ array[2];
			num4 = Tinv0[num4 & 0xFF] ^ Shift(Tinv0[(num8 >> 8) & 0xFF], 24) ^ Shift(Tinv0[(num7 >> 16) & 0xFF], 16) ^ Shift(Tinv0[(num6 >> 24) & 0xFF], 8) ^ array[3];
		}
		array = KW[1];
		num6 = Tinv0[num & 0xFF] ^ Shift(Tinv0[(num4 >> 8) & 0xFF], 24) ^ Shift(Tinv0[(num3 >> 16) & 0xFF], 16) ^ Shift(Tinv0[(num2 >> 24) & 0xFF], 8) ^ array[0];
		num7 = Tinv0[num2 & 0xFF] ^ Shift(Tinv0[(num >> 8) & 0xFF], 24) ^ Shift(Tinv0[(num4 >> 16) & 0xFF], 16) ^ Shift(Tinv0[(num3 >> 24) & 0xFF], 8) ^ array[1];
		num8 = Tinv0[num3 & 0xFF] ^ Shift(Tinv0[(num2 >> 8) & 0xFF], 24) ^ Shift(Tinv0[(num >> 16) & 0xFF], 16) ^ Shift(Tinv0[(num4 >> 24) & 0xFF], 8) ^ array[2];
		num4 = Tinv0[num4 & 0xFF] ^ Shift(Tinv0[(num3 >> 8) & 0xFF], 24) ^ Shift(Tinv0[(num2 >> 16) & 0xFF], 16) ^ Shift(Tinv0[(num >> 24) & 0xFF], 8) ^ array[3];
		array = KW[0];
		C0 = (uint)(Si[num6 & 0xFF] ^ (s[(num4 >> 8) & 0xFF] << 8) ^ (s[(num8 >> 16) & 0xFF] << 16) ^ (Si[(num7 >> 24) & 0xFF] << 24)) ^ array[0];
		C1 = (uint)(s[num7 & 0xFF] ^ (s[(num6 >> 8) & 0xFF] << 8) ^ (Si[(num4 >> 16) & 0xFF] << 16) ^ (s[(num8 >> 24) & 0xFF] << 24)) ^ array[1];
		C2 = (uint)(s[num8 & 0xFF] ^ (Si[(num7 >> 8) & 0xFF] << 8) ^ (Si[(num6 >> 16) & 0xFF] << 16) ^ (s[(num4 >> 24) & 0xFF] << 24)) ^ array[2];
		C3 = (uint)(Si[num4 & 0xFF] ^ (s[(num8 >> 8) & 0xFF] << 8) ^ (s[(num7 >> 16) & 0xFF] << 16) ^ (s[(num6 >> 24) & 0xFF] << 24)) ^ array[3];
	}
}
