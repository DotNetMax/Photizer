var userLang = navigator.language || navigator.userLanguage; 

console.log(userLang);

if(userLang.includes('de'))
{
    setToGermanLanguage();
}
else if(userLang.includes('ja'))
{
    setToJapaneseLanguage();
}
else
{
    setToEnglishLanguage();
}

function setToGermanLanguage()
{
    $('#navigationHome').text('Home');
    $('#navigationFeatures').text('Features');
    $('#navigationDownload').text('Download');
    $('#navigationSupport').text('Unterstützen');

    $('#heroHeading').text('Photos verwalten mit Photizer');
    $('#heroParagraphOne').text('Du möchtest deine Bilder gesammelt an einem Ort auf deinem Computer verwalten, ohne eine aufwändige Ordnerstruktur erstellen zu müssen?');
    $('#heroParagraphTwo').html('Mit Photizer ist es möglich Bildern beim Hinzufügen zur Verwaltung mit Kategorien, Tags, Orten und mehr zu versehen, '
        + 'um diese später leicht wieder zu finden. <a href="#">Hier</a> gibt es einen kleinen Überlick über die Features. Details werden in diesem <a href="https://youtu.be/-VcF9gOBcnM>YouTube Video</a> erklärt');
    
    $('#featureAddPhotosTitle').text('Photos Hinzufügen');
    $('#featureAddPhotosContent').text('Füge dein Bilder zu Photizer hinzu und gebe dabei so viele Information wie möglich an, um es mit der Suchfunktion einfacher zu finden. '
        + 'Einige der Information werden bereits aus den EXIF Daten des Bildes geladen, wie zum Beispiel die Kamera, Objektiv und die Aufnahmeeinstellungen.');
    
    $('#featureSearchTitle').text('Suchfenster');
    $('#featureSearchContent').text('Hier ist das Hauptfenster von Photizer. Mit den angegebenen Informationen beim Hinzufügen können hier '
      + ' die Bilder gesucht werden. Außerdem lassen sie sich in einen Ordner exportieren oder zu einem Album hinzufügen.'
      + ' Bei der Auswahl eines Photos ist es möglich eine Detail Seite aufzurufen.');

    $('#featureViewDetailsTitle').text('Detail Seite');
    $('#featureViewDetailsContent').text('Auf dieser Seite wird das Bild in voller Auflösung dargestellt. Dazu werden darunter die angegebenen Informationen angezeigt.' 
        + ' Diese Informationen lassen sich hier auch bearbeiten.');

    $('#featureCreateCollectionsTitle').text('Alben');
    $('#featureCreateCollectionsContent').text('Hier können Alben erstellt werden denen im Suchfenster dann eine Auswahl von Bildern hinzugefügt werden können. '
        + 'Beim Auswählen eines Albums können alle Bilder in einem seperaten Fenster angezeigt werden. Ein Album kann jederzeit wieder gelöscht werden, die Bilder im Album bleiben erhalten.');

    $('#featurePhotoMapTitle').text('Photo Karte');
    $('#featurePhotoMapContent').text('In den Einstellungen können den Orten, die beim Hinzufügen der Bilder erstellt wurden, Koordinaten hinterlegt werden. '
        + 'Damit ist es möglich sich die Orte auf der Photo Karte mit ihren Bildern anzuzeigen.');

    $('#featureMultilangSupportTitle').text('Mehrsprachige Unterstützung');
    $('#featureMultilangSupportContent').html('Zurzeit ist Photizer auf Englisch, Japanisch und Deutsch erhältlich. Wenn eine weitere Sprache hinzugefügt werden soll kann dazu <a href="#">hier</a> ein "Issue" eröffnet werden.');


    $('#downloadTitle').text('Download Photizer');
    $('#downloadParagraphOne').text('Photizer ist kostenlos und Open Source! Die aktuelle Version kann von der GitHub Release Seite (Download Button) herunterladen werden.');
    $('#downloadParagraphTwo').html('Du hast einen Fehler gefunden oder eine Idee für ein weiteres Features für Photizer? Lege Deine Anforderung bei <a href="#">GitHub</a> als "Issue" an. Jegliches Feedback um Photizer weiter zu verbessern ist erwünscht!');

    $('#supportTile').text('Photizer unterstützen');
    $('#supportContent').text('Photizer wird immer kostenlos und Open Source bleiben. Wenn Dir Photizer gefällt und Du die Entwicklung unterstützen möchtest, würde ich mich sehr über eine Spende freuen!');
}

function setToJapaneseLanguage()
{
    $('#navigationHome').text('ホーム');
    $('#navigationFeatures').text('機能');
    $('#navigationDownload').text('ダウンロード');
    $('#navigationSupport').text('サポートする');


    $('#heroHeading').text('Photizerで写真を整理する');
    $('#heroParagraphOne').text('コンピュータ上の写真を1か所で簡単に管理したいですか？');
    $('#heroParagraphTwo').html('Photizer(フォータイザー)を利用すると、カテゴリ、タグ、場所などを使って写真を追加し、後で簡単に見つけることができます。 '
        + '詳細な概要については、機能のセクションを確認するか、<a href="https://youtu.be/-VcF9gOBcnM">YouTubeビデオ</a>をご覧ください');
    
    $('#featureAddPhotosTitle').text('写真を追加');
    $('#featureAddPhotosContent').text('いくつかのキーワードと一緒に写真をPhotizerに追加すれば検索機能を使って簡単に写真を見つけることができます。'
        + 'カメラ、レンズ、キャプチャ設定など、一部の情報はEXIFデータから読み込まれます');

    $('#featureSearchTitle').text('検索の概要');
    $('#featureSearchContent').text('これはPhotizerのメインウィンドウです。 写真を追加するときに入力した情報を使用して、ここで写真を検索できます。'
        + 'それらをフォルダーにエクスポートしたり、コレクションに追加したりすることができ、写真を選択する際に詳細ページを表示することもできます。');

    $('#featureViewDetailsTitle').text('詳細を見る');
    $('#featureViewDetailsContent').text('このページでは、写真がフル解像度で表示されています。 画像の下に情報が表示されます。 これらのキーワードはここから編集できます。');

    $('#featureCreateCollectionsTitle').text('コレクションを作成する');
    $('#featureCreateCollectionsContent').text('ここでは、検索の概要から選択した写真を追加できるコレクションを作成できます。 これを選択すると、すべての画像を別のウィンドウに表示できます。 コレクションはいつでも削除できますが、その中の写真は保持されます。');

    $('#featurePhotoMapTitle').text('地図');
    $('#featurePhotoMapContent').text('この機能を使用するには、設定メニューの座標を「場所」に追加する必要があります。 次に、地図上の場所を写真で表示することができます。');

    $('#featureMultilangSupportTitle').text('多言語サポート');
    $('#featureMultilangSupportContent').html('現在、Photizerは英語、日本語、ドイツ語でご利用いただけます。 別の言語を追加したい場合は、<a href="#">ここで</a>「Issue」を開くことができます。');

    $('#downloadTitle').text('Photizerをダウンロードする');
    $('#downloadParagraphOne').text('Photizerは無料でオープンソースです！ ダウンロードボタンをクリックすると、GitHubリリースページから現在のリリースをダウンロードできます。');
    $('#downloadParagraphTwo').html('不具合を見つけたり、Photizerの追加機能のアイデアなどがある場合は、<a href="#">GitHub</a>で「Issue」としてリクエストを開きます。 Photizerを改善するためのフィードバックは大歓迎です！');

    $('#supportTile').text('Photizer開発をサポートする');
    $('#supportContent').text('Photizerは常に無料で使えるオープンソースです。開発をサポートしたい場合は、寄付をいただければ幸いです。');
}

function setToEnglishLanguage()
{
    $('#navigationHome').text('Home');
    $('#navigationFeatures').text('Features');
    $('#navigationDownload').text('Download');
    $('#navigationSupport').text('Support');

    
    $('#heroHeading').text('Organize your photos with Photizer');
    $('#heroParagraphOne').text('You want to manage your photos on your computer in one place without having to create a complicated folder structure?');
    $('#heroParagraphTwo').html('Photizer allows you to add your photos with categories, tags, locations etc. to easily find them later. '
        + 'Check the <a href="#">features</a> section or watch the <a href="https://youtu.be/-VcF9gOBcnM">YouTube Video</a> for a detailed overview');
    
    $('#featureAddPhotosTitle').text('Add Photos');
    $('#featureAddPhotosContent').text('Add your photos to Photizer with many keywords to easily find them when you are searching. '
        + 'Some information will be preloaded from the EXIF Data like Camera, Lens and the capture settings.');

    $('#featureSearchTitle').text('Search Overview');
    $('#featureSearchContent').text('This is the main window of Photizer. With the information you enter when adding photos, you can search for '
        + 'them here. You can also export them to a folder or add them to a collection. When selecting a photo, it is possible to oprn up a detail page.');

    $('#featureViewDetailsTitle').text('View Details');
    $('#featureViewDetailsContent').text('On this page the photos is shown in full resolution. Below the image the information is displayed. It is possible to edit these keywords from here.');

    $('#featureCreateCollectionsTitle').text('Create Collections');
    $('#featureCreateCollectionsContent').text('Here you can create collections to which you can add a selection of photos from the search overview. When selecting a collection, all images can be displayed in a separate window. A collection can be deleted at any time, the photos in the collection will be kept.');

    $('#featurePhotoMapTitle').text('Photo Map');
    $('#featurePhotoMapContent').text('In the settings, coordinates can be added to the locations that were created when the photos were added. Then it is possible to show the places on the photo map with their photos.');

    $('#featureMultilangSupportTitle').text('Multi Language Support');
    $('#featureMultilangSupportContent').html('Right now Photizer is available in English, Japanese and German. If you want to have another language added you can open an "issue" <a href="#">here</a>.');


    $('#downloadTitle').text('Download Photizer');
    $('#downloadParagraphOne').text('Photizer is free and Open Source! You can download the current release from the GitHub Release Page by clicking on the download button.');
    $('#downloadParagraphTwo').html('You found a bug or have an idea for an additional feature for Photizer? Then open your request as an "issue" on <a href="#">GitHub</a>. Any feedback to improve Photizer is welcome!');


    $('#supportTile').text('Support the Photizer Development');
    $('#supportContent').text('Photizer will always be free and Open Source. If you like Photizer and you want to support the development i would be very happy for your donation!');
}

