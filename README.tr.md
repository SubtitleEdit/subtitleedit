# Subtitle Edit

Altyazı düzenleyici :)

[English](README.md) | [Türkçe](README.tr.md)

---

## 🌐 Dokümantasyon ve SSS
https://subtitleedit.github.io/subtitleedit/

---

## 🚀 Otomatik Derlemeler
En güncel platformlar arası derlemeleri burada bulabilirsiniz:  
👉 [Sürümler](https://github.com/SubtitleEdit/subtitleedit/releases)

---

## 💻 Sistem Gereksinimleri

### Windows
- Minimum: Windows 10 sürüm 22H2 (derleme 19045) veya daha yeni, tamamen güncellenmiş bir sürüm. Daha eski Windows 10 derlemeleri (2004/20H2/21H1/21H2) kullanım ömrünün sonuna gelmiştir ve bir .NET çalışma zamanı hatası (`0x80131506`) nedeniyle başlatılamayabilir.

### macOS

- **Minimum macOS sürümü**: 12 (Monterey) veya daha yeni
- **Önerilen**: macOS 14 (Sonoma) veya daha yeni - .NET 10'un desteklendiği en eski macOS sürümü. Subtitle Edit 12 ve 13 sürümlerinde de çalışmaya devam eder, ancak bu kombinasyon .NET çalışma zamanı tarafından test edilen yapılandırmalardan biri değildir.
- `.dmg` dosyası kendi içinde gerekli bileşenleri barındırır: `libmpv` ve `ffmpeg`, `Subtitle Edit.app` içine dahil edilmiştir; bu nedenle MacPorts veya Homebrew kurulumu gerekmez.

#### macOS'ta Subtitle Edit kurulumu

**v5.2.0** itibarıyla `.dmg` dosyası bir Apple Developer ID ile imzalanmış ve Apple tarafından noterize edilmiştir; bu nedenle normal şekilde açılır ve Terminal üzerinden karantina kaldırma adımı gerekmez:

1. `.dmg` dosyasını **indirin** ve bağlamak için **çift tıklayın**.
2. Açılan pencerede **`Subtitle Edit.app` dosyasını `Applications` klasörünüze sürükleyin**.
3. **Applications** klasöründen (veya Launchpad'den) **Subtitle Edit'i** açın.

### Linux

#### Flatpak (her dağıtım)

[Releases](https://github.com/SubtitleEdit/subtitleedit/releases) sayfasından bir Flatpak paketi edinilebilir. Gerekli tüm bağımlılıkları (mpv, ffmpeg) içerir; ayrıca kurulum gerekmez.

```bash
flatpak install SubtitleEdit-linux-x64.flatpak
flatpak run dk.nikse.subtitleedit
```

#### Yerel derlemeler (`.tar.gz`)

Video işlevlerini etkinleştirmek için mpv ve ffmpeg gereklidir (ffmpeg genellikle zaten kuruludur).

##### Debian/Ubuntu
```bash
sudo apt update && sudo apt install -y mpv libmpv-dev ffmpeg
```

##### Arch
```bash
sudo pacman -S mpv ffmpeg
```

##### Fedora
```bash
sudo dnf install mpv-libs ffmpeg-free
```
(veya [RPM Fusion](https://rpmfusion.org/) üzerinden `ffmpeg`)

##### openSUSE
```bash
sudo zypper install mpv ffmpeg
```

> ⚙️ Not: Sağlanan derlemeler kendi içinde gerekli bileşenleri barındırır ve ayrıca bir .NET kurulumu gerektirmez.

---

## 🔒 Gizlilik

**Subtitle Edit**, çevrimdışı çalışan açık kaynaklı bir uygulamadır.  
Altyazı dosyalarınızın, medya dosyalarınızın veya bunlarla ilişkili herhangi bir meta verinin içeriğini **toplamaz, saklamaz, iletmez veya analiz etmez** — ne analiz amacıyla, ne model eğitimi için, ne de şimdi veya gelecekte başka herhangi bir ikincil amaçla.

Düzenleme, dönüştürme, video oynatma ve **yerel otomatik yedekleme** dahil olmak üzere tüm temel özellikler tamamen cihazınızda çalışır.

Subtitle Edit içinde isteğe bağlı üçüncü taraf çevrimiçi hizmetleri (örneğin çeviri, konuşmayı metne dönüştürme, metinden konuşmaya dönüştürme, OCR veya sözlük/arama hizmetleri) kullanmayı seçerseniz, yalnızca ilgili isteği gerçekleştirmek için gereken asgari veri doğrudan seçtiğiniz sağlayıcıya gönderilir. Bu tür veri aktarımları sağlayıcının kendi gizlilik politikasına tabidir ve Subtitle Edit bu verileri hiçbir şekilde saklamaz veya başka bir yere iletmez.

Subtitle Edit, dosyalarınız üzerinde tam kontrol sahibi olmanızı amaçlar — verileriniz size aittir.

---

## ❤️ Projeyi Destekleyin
Subtitle Edit'in geliştirilmesini desteklemek isterseniz bağış yapmayı düşünebilirsiniz:

- [GitHub Sponsors](https://github.com/sponsors/niksedk)
- [PayPal ile bağış yapın](https://www.paypal.com/donate/?hosted_button_id=4XEHVLANCQBCU)

---
