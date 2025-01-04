include Vars.mk

TARGETS = win-release win-debug win-install linux-release win-distro linux-distro

all: $(TARGETS)

dirs: $(BIN_DIR) $(DEBUG_DIR) $(RELEASE_DIR)

$(BIN_DIR):
	if not exist $(BIN_DIR) mkdir $(BIN_DIR)

$(WIN_DEBUG_DIR): $(BIN_DIR)
	if not exist $(WIN_DEBUG_DIR) mkdir $(WIN_DEBUG_DIR)

$(WIN_RELEASE_DIR): $(BIN_DIR)
	if not exist $(WIN_RELEASE_DIR) mkdir $(WIN_RELEASE_DIR)

$(LINUX_RELEASE_DIR): $(BIN_DIR)
	if not exist $(LINUX_RELEASE_DIR) mkdir $(LINUX_RELEASE_DIR)

$(WIN_INSTALLER_DIR): $(BIN_DIR)
	if not exist $(WIN_INSTALLER_DIR) mkdir $(WIN_INSTALLER_DIR)

win-release: $(WIN_RELEASE_DIR)
	$(GODOT) --path $(GAME_DIR) --export-release "Windows Desktop" ..\Installer\$(WIN_RELEASE_DIR)\$(ASTEROIDS_EXE) --display-driver headless --audio-driver Dummy

win-debug: $(WIN_DEBUG_DIR)
	$(GODOT) --path $(GAME_DIR) --export-debug "Windows Desktop" ..\Installer\$(WIN_DEBUG_DIR)\$(ASTEROIDS_EXE) --display-driver headless --audio-driver Dummy

linux-release: $(LINUX_RELEASE_DIR)
	$(GODOT) --path $(GAME_DIR) --export-release "Linux" ..\Installer\$(LINUX_RELEASE_DIR)\$(ASTEROIDS_EXE) --display-driver headless --audio-driver Dummy

win-install: win-release
	$(ISCC) /O$(BIN_DIR) /Qp \
		-DVERSION=$(VERSION) -DEXE_NAME=$(ASTEROIDS_EXE) \
		-DAPP_NAME=$(APP_NAME) -DINSTALLER_DIR=$(BIN_DIR) \
		-DRELEASE_DIR=$(WIN_RELEASE_DIR) -DWIZARD_IMAGE=$(ASSETS_DIR)\WizardImage.bmp \
		-DWIZARD_SMALL_IMAGE=$(ASSETS_DIR)\WizardSmallImage.bmp \
		Installer.iss

win-distro: win-release
	cd  $(WIN_RELEASE_DIR) & ${ZIP} a -tzip -r ..\$(APP_NAME)-win-$(VERSION).zip .

linux-distro: linux-release
	cd  $(LINUX_RELEASE_DIR) & ${ZIP} a -tzip -r ..\$(APP_NAME)-linux-$(VERSION)-linux.zip .

clean:
	rmdir /S /Q $(BIN_DIR)
