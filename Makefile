rwildcard = $(wildcard $1$2) $(foreach d,$(wildcard $1*),$(call rwildcard,$d/,$2))

TARGET = $(BUILD_DIR)/robol.exe
SRC_DIR = src
BUILD_DIR = build
SOURCES := $(call rwildcard,$(SRC_DIR)/,*.cs)

CSC_FLAGS = -errorendlocation

FILE ?= programs/example.rbl

.PHONY: all
all: $(TARGET)

$(TARGET): $(SOURCES)
	@mkdir -p $(BUILD_DIR)
	@csc $(SOURCES) -out:$(TARGET) $(CSC_FLAGS)

.PHONY: run
run: $(TARGET)
	@mono $(TARGET) $(FILE)

clean:
	@rm -rf $(BUILD_DIR)
