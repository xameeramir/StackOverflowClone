function speak(inputText) {
    var msg = new SpeechSynthesisUtterance(inputText);
    msg.rate = 0.7;
    msg.pitch = 1;
    window.speechSynthesis.speak(msg);
}