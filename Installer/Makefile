include Vars.mk

TARGETS = release debug install linux-release distro linux-distro

all: $(TARGETS)

dirs: $(BIN_DIR) $(DEBUG_DIR) $(RELEASE_DIR)

$(BIN_DIR):
	if not exist $(BIN_DIR) mkdir $(BIN_DIR)

$(DEBUG_DIR): $(BIN_DIR)
	if not exist $(DEBUG_DIR) mkdir $(DEBUG_DIR)

$(RELEASE_DIR): $(BIN_DIR)
	if not exist $(RELEASE_DIR) mkdir $(RELEASE_DIR)

$(LINUX_DIR): $(BIN_DIR)
	if not exist $(LINUX_DIR) mkdir $(LINUX_DIR)

$(INSTALLER_DIR): $(BIN_DIR)
	if not exist $(INSTALLER_DIR) mkdir $(INSTALLER_DIR)

release: $(RELEASE_DIR)
	$(GODOT) --path $(GAME_DIR) --export-release "Windows Desktop" ..\Installer\$(RELEASE_DIR)\$(ASTEROIDS_EXE) --display-driver headless --audio-driver Dummy

debug: $(DEBUG_DIR)
	$(GODOT) --path $(GAME_DIR) --export-debug "Windows Desktop" ..\Installer\$(RELEASE_DIR)\$(ASTEROIDS_EXE) --display-driver headless --audio-driver Dummy

linux-release: $(LINUX_DIR)
	$(GODOT) --path $(GAME_DIR) --export-release "Linux" ..\Installer\$(LINUX_DIR)\$(ASTEROIDS_EXE) --display-driver headless --audio-driver Dummy

install: release
	$(ISCC) /O$(BIN_DIR) /Qp \
		-DVERSION=$(VERSION) -DEXE_NAME=$(ASTEROIDS_EXE) \
		-DAPP_NAME=$(APP_NAME) -DINSTALLER_DIR=$(BIN_DIR) \
		-DRELEASE_DIR=$(RELEASE_DIR) -DWIZARD_IMAGE=$(ASSETS_DIR)\WizardImage.bmp \
		-DWIZARD_SMALL_IMAGE=$(ASSETS_DIR)\WizardSmallImage.bmp \
		Installer.iss

distro: release
	cd  $(RELEASE_DIR) & ${ZIP} a -tzip -r ..\$(APP_NAME)-$(VERSION).zip .

linux-distro: linux-release
	cd  $(LINUX_DIR) & ${ZIP} a -tzip -r ..\$(APP_NAME)-$(VERSION)-linux.zip .

clean:
	rmdir /S /Q $(BIN_DIR)
