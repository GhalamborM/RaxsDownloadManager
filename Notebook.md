
Finglish (Persian [ Farsi ] with english type) notebook for understanding the flow of HttpClient.

Kar kardan ba HttpClient vaghean sakhte, chon handle kardan ye seri chiza tush gonge [ la aghal baraye man ]. 
Baraye hamin ham harchi yad migiram ro inja minevisam ta kasaei ke mikhan ina ro yad begiran va donbale ye marja'e khob migardan, estefade bebaran.

Ye nokte begam ke hameye api haye networki ke be Http va Ftp rabt daran, az hamin ravesh haye zir estefade mikonan, pas khondane in mataaleb khali az lotf nist.

Hala chera farsi type nakardam? chon farsi type kardane man ye khorde kond hast vase hamin finglish type mikonam ke sari tar in project pish bere.

1. HttpClient ye classe Disposable hast va hatman bayad bad az etmaame kar ba oun, classesh ro dispose konim, ya mostaghim az kalameye kilidie `using`
   estefade kard, ta khodkar dispose beshe. Taghriban har classi ke marbot be HttpClient hast Disposable hastan, pas dispose kardaneshon ro be hich ounvan
   yadeton nare. 


2. HttpClient vase file haei ke bish az 2GB yani 2147483647 bytes [meghdare int.MaxValue hast] dare Exception mide, hata age requeste HEAD befresti.

[Dalil] =>			Dalilesh ine ke HttpClient ye bug dare ke hata age vase url ha darkhaste HEAD ham befresti, bazam say mikone file ro GET kone.
	hata age darkhaste GET e khali ham befresti bazam exception mide, dalilesh ine ke HttpClient ejaze nemide ke bish az 2GB ro berizi tuye Memory.
	shayad be zehneton berese ke property e	`MaxResponseContentBufferSize` e HttpClient long hast pas mishe	`long.MaxValue` behesh dad. ke bayad arz konam
	doroste ke long hast ama vaghti bekhaid ye adad ke bish az int.MaxValue bashe ro tanzim konid, beheton Exception mide ke nemitonid bish az 2147483647
	tanzim konid.
  
[Rahe hal 1] =>		Ye rahe hal ine ke az `HttpCompletionOption.ResponseHeadersRead` estefade kard injori HttpClient faghat header haye khali ro mikhone
	va be hich ounvan nemiad Stream e daryafti ro read kone [ ta zamani ke khodemon biaim readesh ro start konim]. 
	Moshkele in rahe hal ine ke be hich ounvan nemishe fahmid ke ye file `Partial` ya `206` hast ya na, va hamishe javab `OK` ya `200`.
	Hala response e `Partial` ya `206` chie? in response daghighan miad be ma mifahmone ke aya ye file ghabeliyat chanding connection shodan dare ya na.
	Ma'nie harfe bala ine ke aya mishe download ro part part kard ya kheyr, ke tuye in rahe hal javab kheyr ast chon nemitonid befahmid ke partial hast ya na.

[Rahe hal 2] =>		Rahe hale manteghi ke hameye downloader haye ma'rof estefade mikonan ine ke ye requeste GET ba range 0 ta 255 byte mifrestan.
	Injori bedone exception ham header ro mikhone ham stream e downloadi ke 255 byte hast az server khonde mishe.


[Soal 1] Hala chera mian yekbar darkhaste GET e khali mifrestan bad shoro mikonan download?

[Javab] ine ke ba requeste GET e avale kar ye seri value ha ro az server migiran, nemonash `ContentLength` ya hamon hajme file downloadi 
	hast ke age null bashe ya tuye bazi client ha -1 bashe yani ContentLength az tarafe server, naamoshakhas hast. 
	Ye dalile dg ke dare ine ke ba range 0 ta 255, mishe taskhis dad ke ye file `Partial` hast ya na. 
	Ye nokte ke bayad dar nazar dashte bashid ine ke ye download mitone `Partial` bashe ama size e downloadi naamoshakhas bashe.
	Hala shayad soali ke baraton pish biad ine ke downloader ha dg chia ro az server migiran? 
	`ContentType` => Ke moshakhas mikone type e downloadi chi hast va mime type e file ro moshakhas mikone.
	`ContentDisposition.Size` => Az in yeki ham mishe baraye daryafte hajme file estefade kard ama mamolan `null` hast, `ContentDisposition` ham mitone `null` bashe.
	`ContentDisposition.FileName` => File name ei ke az tarafe server mitone tanzim shode bashe va ba gereftane name az linke fili ke ma darim
		mitone motefaavet bashe. Yadeton bashe ke in `FileName` mitone `null` bashe, hata `ContentDisposition` ham mitone `null` bashe.
	`ContentDisposition.ModificationDate` => Tarikhe e modify shodane file, in bishtar be darde download haye `Partial` mikhore ke age, tuye
		server file e downloadi hajmesh avaz shod ya ye balaei saresh omad, ma motevajeh beshim va koliye part haye downloadi ro hazf konim va
		az sefr shoro be dl konim.


[Soal 2] Manzoram az in harf ke bala goftam chie? `range 0 ta 255 byte mifrestan`.

[Javab] Ba estefade az `HttpRequestMessage.Headers.Range` mishe taein kard ke HttpClient az koja ta koja download kone, in value pishfarz `null` hast.
	`Range` yeki az mohem tarin mabhas dar download yek file hast. 
	Hala tarighe karesh chetorie? Be sorate `HttpRequestMessage.Headers.Range = new RangeHeaderValue(0, 255)` tarif mishe.
	Yadeton bashe ke value aval ke `FROM` ya `az` va value dovom ke `TO` ya `ta` ro moshakhas mikone mitonan ke `null` bashan. 
	Be ebarati `Nullable<long>` hastan.


	
	

File haye `M3U` ya `M3U8` ye playlist az stream haye segment shode hastan. bayad check kard ke age link dakhelesh hast link ro dl kard, age na khode
file e m3u8 ro dl kard.
File e `m3u8` be sorate zir hast>
```
#EXTM3U
#EXT-X-TARGETDURATION:12
#EXT-X-ALLOW-CACHE:YES
#EXT-X-PLAYLIST-TYPE:VOD
#EXT-X-VERSION:3
#EXT-X-MEDIA-SEQUENCE:1
#EXTINF:4.000,
https://exmaple.com/seg-1-v1-a1.ts
#EXTINF:12.000,
https://exmaple.com/seg-2-v1-a1.ts
#EXT-X-ENDLIST
```
Ba estefade az nuget e `m3uParser` mishe `M3U8` ro parse kard, ama khob chon man nemikham az package haye dg estefade konam, az in package estefade nemikonam.
Be jash az regex e `@"#EXTINF:(?<duration>.*),\r\n(?<link>((https|http|www.)?\S+))"` estefade mikonam.