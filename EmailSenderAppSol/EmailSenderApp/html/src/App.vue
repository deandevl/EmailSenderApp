<template>
  <div class="emailComp">

    <section class="emailComp_emailsSec">
      <section class="emailComp_tableSec">
        <section class="emailComp_databaseSec">
          <button-comp
              v-on:button_comp_clicked="select_dbfolder">Database Folder...
          </button-comp>
          <label-comp
              heading="Database Folder"
              header_position="above"
              :value="db_folder"
              :css_variables="label_css">
          </label-comp>
          <select-edit-comp
              heading="Database"
              placeholder="Select/Add a database"
              input_size="20"
              :items="db_file_names"
              :single_border="single_border"
              :css_variables="select_edit_css"
              v-on:select_edit_comp_value_changed="db_changed">
          </select-edit-comp>
          <select-comp
              heading="Group"
              placeholder="Select a group"
              :items="group_names"
              :select_value="group_name"
              :single_border="single_border"
              :css_variables="select_css"
              v-on:select_comp_value_changed="group_changed">
          </select-comp>
        </section>

        <table-comp
            class="emailComp_tableComp"
            title="Email Addresses"
            :rows="table_email_addresses"
            :headings="table_headings"
            :column_widths="table_column_widths"
            :css_variables="table_css"
            v-on:table_comp_row="table_row_selected">
        </table-comp>

        <section class="emailComp_inputSec">
          <select-edit-comp
              heading="Group"
              placeholder="Select/Add a group"
              input_size="18"
              :items="group_names"
              :select_value="input_email_group_name"
              :single_border="single_border"
              :css_variables="select_edit_css"
              v-on:select_edit_comp_value_changed="email_group_name_changed">
          </select-edit-comp>
          <input-comp
              heading="Name"
              placeholder="Enter email name"
              header_position="above"
              input_size="24"
              :single_border="input_single_border"
              :input_value="input_email_name"
              :css_variables="input_css"
              v-on:input_comp_value_changed="email_name_changed">
          </input-comp>
          <input-comp
              heading="URL Address"
              placeholder="Enter url"
              header_position="above"
              input_size="30"
              input_type="email"
              :single_border="input_single_border"
              :input_value="input_email_url"
              :css_variables="input_css"
              v-on:input_comp_value_changed="email_url_changed">
          </input-comp>
        </section>

        <section class="emailComp_inputButtonSec">
          <button-comp
              v-on:button_comp_clicked="add_address_clicked">Add
          </button-comp>
          <button-comp
              v-on:button_comp_clicked="update_address_clicked">Update
          </button-comp>
          <button-comp
              v-on:button_comp_clicked="delete_address_clicked">Delete
          </button-comp>
          <button-comp
              v-on:button_comp_clicked="cancel_address_clicked">Cancel
          </button-comp>
        </section>
      </section>

      <section class="emailComp_messageSec">
        <section class="emailComp_messageButtonSec">
          <button-comp
              v-on:button_comp_clicked="set_message_file">Set Message File...
          </button-comp>
          <button-comp
              v-on:button_comp_clicked="set_image_file">Set Image File...
          </button-comp>
          <button-comp
              v-on:button_comp_clicked="check_addresses">Check Addresses
          </button-comp>
          <button-comp
              v-on:button_comp_clicked="send_emails">Send Emails
          </button-comp>
        </section>
        <section class="emailComp_infoSec">
          <input-comp
              heading="Email Subject:"
              placeholder="Enter a subject"
              :input_value="input_email_subject"
              :single_border="input_single_border"
              :css_variables="input_css"
              v-on:input_comp_value_changed="email_subject_changed">
          </input-comp>
          <label-comp
              heading="Message File:"
              :value="message_file_path"
              :css_variables="label_info_css">
          </label-comp>
          <label-comp
              heading="Image File:"
              :value="image_file_path"
              :css_variables="label_info_css">
          </label-comp>
        </section>
        <table-comp
            class="emailComp_tableComp"
            title="Bad Email Addresses"
            :rows="table_bad_email_rows"
            :headings="table_headings"
            :column_widths="table_column_widths"
            :css_variables="table_css">
        </table-comp>
      </section>
    </section>
    <section class="emailComp_statusSec">{{status_content}}</section>
  </div>
</template>

<script>
  import Vue from 'vue';
  import ButtonComp from 'buttoncomp';
  import InputComp from 'inputcomp';
  import LabelComp from 'labelcomp';
  import SelectEditComp from 'selecteditcomp';
  import SelectComp from 'selectcomp';
  import TableComp from 'tablecomp';
  
  export default {
    name: "App",
    data: function() {
      return {
        localhost: 'http://localhost:8082/emailsender',

        db_folder: null,
        db_file_names: null,
        db_files: null,

        email_bus: new Vue(),

        crud_action: null,
        group_names: null,
        group_name: null,
        message_file_path: null,
        image_file_path: null,

        backup_address: null,

        current_table_row: null,

        //inputs
        input_email_name: null,
        input_email_group_name: null,
        input_email_url: null,
        input_email_subject: null,

        status_content: "Status",

        select_css: {
          select_comp_heading_color: "white",
          select_comp_color: "white",
          select_comp_arrow_color: "white",
          select_comp_border_color: "white",
          select_comp_items_panel_color: "white",
          select_comp_items_panel_background: "#2E1076",
          select_comp_item_hover_color: "gold"
        },

        table_headings: ['group','name','url'],
        table_column_widths: [120,120,200],
        table_css: {
          table_comp_title_color: "white",
          table_comp_thead_color: "white",
          table_comp_thead_border_bottom: "1px solid white",
          table_comp_thead_background: "transparent",
          table_comp_tbody_height: "220px",
          table_comp_row_color: "white",
          table_comp_row_odd_background: "transparent",
          table_comp_row_even_background: "transparent",
          table_comp_row_border_bottom: "1px solid white",
        },
        table_email_addresses: null,
        table_bad_email_rows: null,

        select_edit_css: {
          select_edit_heading_color: "white",
          select_edit_color: "white",
          select_edit_placeholder_color: "white",
          select_edit_arrow_color: "white",
          select_edit_border_color: "white",
          select_edit_items_panel_color: "white",
          select_edit_items_panel_background: "#2E1076",
          select_edit_items_panel_border: "1px solid white",
          select_edit_item_hover_color: "gold"
        },

        input_single_border: true,
        input_css: {
          input_comp_heading_color: "white",
          input_comp_input_color: "white",
          input_comp_input_border_color: "white",
          input_comp_input_placeholder_color: "white",
          input_comp_input_focus_outline: "gold",
          input_comp_input_focus_background: "transparent"
        },

        label_css: {
          label_comp_heading_color: 'white',
          label_comp_value_color: 'white'
        },

        label_info_css: {
          label_comp_heading_color: 'white',
          label_comp_value_color: 'white',
          label_comp_value_width: '32rem'
        },

        single_border: true
      }
    },

    components: {
      ButtonComp,
      InputComp,
      LabelComp,
      SelectEditComp,
      SelectComp,
      TableComp
    },
    mounted() {
      //set up 'status_changed' event
      this.email_bus.$on('status_changed', (message) => {
        this.status_content=message;
      });
    },
    methods: {
      //select database folder
      select_dbfolder: function() {
        const url=this.localhost;
        const request_data={
          action: 'getDbFiles'
        };
        const request_data_str=JSON.stringify(request_data);
        const config={
          method: 'POST',
          mode: 'cors',
          body: request_data_str,
          headers: new Headers({
            'Content-Type': 'application/json',
            'Content-Length': request_data_str.length
          })
        };
        fetch(url, config).then(response => {
          if(response.ok) {
            return response.text();
          }
          throw new Error(response.statusText);
        }).then(resp_str => {
          const resp_dict=JSON.parse(resp_str);
          this.db_folder = resp_dict.dbfolder;

          //set SelectEditComp items (database file names) in database folder
          this.db_files = {};
          const file_paths = resp_dict.dbfilepaths;
          this.db_file_names = resp_dict.dbfilenames;
          for(let i=0; i<this.db_file_names.length; i++){
            this.db_files[this.db_file_names[i]] = file_paths[i];
          }
        }).catch(error => {
          this.email_bus.$emit('status_changed', `Get database files error: ${error.message}`)
        });
      },
      db_changed: function(value) {
        const db_name=value;
        if(db_name!==null) {
          let db_path=null;
          if(this.db_files[db_name]!==undefined) {
            db_path=this.db_files[db_name];
          } else {
            if(db_name.indexOf('.db')!== -1) {
              db_path=`${this.db_folder}\\${db_name}`;
            } else {
              db_path=`${this.db_folder}\\${db_name}.db`;
            }
          }

          //select database name
          const url=this.localhost;
          const request_data={
            action: 'selectdb',
            dbpath: db_path
          };
          const request_data_str=JSON.stringify(request_data);
          const config={
            method: 'POST',
            mode: 'cors',
            body: request_data_str,
            headers: new Headers({
              'Content-Type': 'application/json',
              'Content-Length': request_data_str.length
            })
          };
          fetch(url, config).then(response => {
            if(response.ok) {
              return response.text();
            }
            throw new Error(response.statusText);
          }).then(resp_str => {
            this.group_names=JSON.parse(resp_str);
            this.email_bus.$emit('status_changed',`Server selected ${db_path}`);
          }).catch(error => {
            this.email_bus.$emit('status_changed', `Get group names error: ${error.message}`)
          });
        }
      },
      group_changed: function(value) {
        if(value !== null){
          this.group_name = value;
          this.get_addresses();
        }
      },
      table_row_selected: function(obj) {
        this.input_email_group_name = obj.row_values[0];
        this.input_email_name = obj.row_values[1];
        this.input_email_url = obj.row_values[2];
        this.current_table_row = obj.row_values;

      },
      email_group_name_changed: function(value) {
        this.input_email_group_name = value;
      },
      email_subject_changed: function(value){
        this.input_email_subject = value;
      },
      email_name_changed: function(value){
        this.input_email_name = value;
      },
      email_url_changed: function(value){
        this.input_email_url = value;
      },
      add_address_clicked: function(){
        const address = {};
        address.GroupName = this.input_email_group_name;
        address.Name = this.input_email_name;
        address.Url = this.input_email_url;
        this.add_address(address,false);
      },
      add_address: function(address, isCancel) {
        this.crud_action = 'Add';
        const url=this.localhost;
        const request_data = {
          action: 'addAddress',
          address: address
        };
        const request_data_str=JSON.stringify(request_data);
        const config={
          method: 'POST',
          mode: 'cors',
          body: request_data_str,
          headers: new Headers({
            'Content-Type': 'application/json',
            'Content-Length': request_data_str.length
          })
        };
        fetch(url, config).then(response => {
          if(response.ok){
            return response.text();
          }
          throw new Error(response.statusText);
        }).then(resp_str => {
          //returns both a list of group names and backup address
          const address_obj = JSON.parse(resp_str);
          this.group_names = address_obj.group_names;
          this.group_name = this.input_email_group_name;
          this.backup_address = JSON.parse(JSON.stringify(address_obj.backup_address));

          //reset the table
          this.get_addresses();
          if(isCancel){
            this.email_bus.$emit('status_changed', `Successfully cancelled delete address ${address.Name}`);
          }else {
            this.email_bus.$emit('status_changed', `Successfully added address ${address.Name}`);
          }
        }).catch(error => {
          this.email_bus.$emit('status_changed', `Add address error: ${error.message}`)
        });
      },
      delete_address_clicked: function(){
        const address = {};
        address.GroupName = this.input_email_group_name;
        address.Name = this.input_email_name;
        address.Url = this.input_email_url;
        this.delete_address(address,false);
      },
      delete_address: function(address, isCancel) {
        this.crud_action = 'Delete';
        const url = this.localhost;
        const request_data = {
          action: 'deleteAddress',
          address: address
        };
        const request_data_str=JSON.stringify(request_data);
        const config={
          method: 'POST',
          mode: 'cors',
          body: request_data_str,
          headers: new Headers({
            'Content-Type': 'application/json',
            'Content-Length': request_data_str.length
          })
        };
        fetch(url, config).then(response => {
          if(response.ok){
            return response.text();
          }
          throw new Error(response.statusText);
        }).then(resp_str => {
          const resp_dict=JSON.parse(resp_str);

          //returns both a list of group names and backup address
          const address_obj = JSON.parse(resp_str);
          this.group_names = address_obj.group_names;
          this.backup_address = JSON.parse(JSON.stringify(address_obj.backup_address));

          //reset the table
          this.group_name = address.GroupName;
          if(this.group_names.length > 0 && this.group_names.indexOf(this.group_name) === -1) {
            this.group_name = null;
            this.table_email_addresses = [];
          }else {
            this.get_addresses();
          }

          if(isCancel){
            this.email_bus.$emit('status_changed', `Successfully cancelled add address ${address.Name}`);
          }else {
            this.email_bus.$emit('status_changed', `Successfully deleted address ${address.Name}`);
          }
        }).catch(error => {
          this.email_bus.$emit('status_changed', `Update address error: ${error.message}`)
        });
      },
      update_address_clicked: function(){
        const address = {};
        address.currentGroupName = this.current_table_row[0];
        address.updateGroupName = this.input_email_group_name;
        address.currentName = this.current_table_row[1];
        address.updateName = this.input_email_name;
        address.Url = this.input_email_url;
        this.update_address(address,false);
      },
      update_address: function(address, isCancel) {
        this.crud_action = 'Update';
        const url = this.localhost;
        const request_data = {
          action: 'updateAddress',
          address: address
        };
        const request_data_str=JSON.stringify(request_data);
        const config = {
          method: 'POST',
          mode: 'cors',
          body: request_data_str,
          headers: new Headers({
            'Content-Type': 'application/json',
            'Content-Length': request_data_str.length
          })
        };
        fetch(url, config).then(response => {
          if(response.ok){
            return response.text();
          }
          throw new Error(response.statusText);
        }).then(resp_str => {
          //returns both a list of group names and backup address
          const address_obj = JSON.parse(resp_str);
          this.group_names = address_obj.group_names;
          this.backup_address = JSON.parse(JSON.stringify(address_obj.backup_address));
          this.group_name = address.updateGroupName;

          //reset the table
          if(this.group_names.length > 0 && this.group_names.indexOf(this.group_name) === -1) {
            this.group_name = null;
            this.table_email_addresses = [];
          }else {
            this.get_addresses();
          }

          if(isCancel){
            this.email_bus.$emit('status_changed', `Successfully cancelled updated address ${address.currentName}`);
          }else {
            this.email_bus.$emit('status_changed', `Successfully updated address ${address.currentName}`);
          }
        }).catch(error => {
          this.email_bus.$emit('status_changed', `Update address error: ${error.message}`)
        });
      },
      cancel_address_clicked: function() {
        switch(this.crud_action) {
          case 'Add':
            this.delete_address(this.backup_address, true);
            break;
          case 'Update':
            const address = {};
            address.currentGroupName = this.input_email_group_name;
            address.updateGroupName = this.backup_address.GroupName;
            address.currentName = this.input_email_name;
            address.updateName = this.backup_address.Name;
            address.Url = this.backup_address.Url;
            this.update_address(address, true);
            break;
          case 'Delete':
            this.add_address(this.backup_address, true);
            break;
        }
      },
      get_addresses: function() {
        if(this.group_name !== null) {
          const url = this.localhost;
          const request_data = {
            action: 'getAddresses',
            groupname: this.group_name
          };
          const request_data_str = JSON.stringify(request_data);
          const config = {
            method: 'POST',
            mode: 'cors',
            body: request_data_str,
            headers: new Headers({
              'Content-Type': 'application/json',
              'Content-Length': request_data_str.length
            })
          };
          fetch(url, config).then(response => {
            if(response.ok) {
              return response.text();
            }
            throw new Error(response.statusText);
          }).then(resp_str => {
            this.table_email_addresses = [];
            const address_array = JSON.parse(resp_str);
            for(let address of address_array) {
              const row = [
                [address.GroupName,''],
                [address.Name,''],
                [address.Url,'']
              ];
              this.table_email_addresses.push(row);
            }
          }).catch(error => {
            this.email_bus.$emit('status_changed', `Get addresses error: ${error.message}`)
          });
        }
      },
      set_message_file: function() {
        const url = this.localhost;
        const request_data = {
          action: 'getMessageFilePath'
        };
        const request_data_str = JSON.stringify(request_data);
        const config = {
          method: 'POST',
          mode: 'cors',
          body: request_data_str,
          headers: new Headers({
            'Content-Type': 'application/json',
            'Content-Length': request_data_str.length
          })
        };
        fetch(url, config).then(response => {
          if(response.ok){
            return response.text();
          }
          throw new Error(response.statusText);
        }).then(resp_str => {
          this.message_file_path = resp_str;
        }).catch(error => {
          this.email_bus.$emit('status_changed', `Get message file path error: ${error.message}`)
        });
      },
      set_image_file: function() {
        const url = this.localhost;
        const request_data = {
          action: 'getImageFilePath'
        };
        const request_data_str = JSON.stringify(request_data);
        const config={
          method: 'POST',
          mode: 'cors',
          body: request_data_str,
          headers: new Headers({
            'Content-Type': 'application/json',
            'Content-Length': request_data_str.length
          })
        };
        fetch(url, config).then(response => {
          if(response.ok){
            return response.text();
          }
          throw new Error(response.statusText);
        }).then(resp_str => {
          this.image_file_path = resp_str;
        }).catch(error => {
          this.email_bus.$emit('status_changed', `Get image file path error: ${error.message}`)
        });
      },
      check_addresses: function() {
        if(this.group_name !== null) {
          const url = this.localhost;
          const request_data = {
            action: 'checkAddresses',
            groupname: this.group_name
          };
          const request_data_str=JSON.stringify(request_data);
          const config = {
            method: 'POST',
            mode: 'cors',
            body: request_data_str,
            headers: new Headers({
              'Content-Type': 'application/json',
              'Content-Length': request_data_str.length
            })
          };
          fetch(url, config).then(response => {
            if(response.ok) {
              return response.text();
            }
            throw new Error(response.statusText);
          }).then(resp_str => {
            this.table_bad_email_rows=[];
            const address_array=JSON.parse(resp_str);
            for(let address of address_array) {
              const row = [
                [address.GroupName,''],
                [address.Name,''],
                [address.Url,'']
              ];
              this.table_bad_email_rows.push(row);
            }
            this.email_bus.$emit('status_changed',`Found ${this.table_bad_email_rows.length} bad address(es) for ${this.group_name}`);
          }).catch(error => {
            this.email_bus.$emit('status_changed', `Check addresses error: ${error.message}`)
          });
        }
      },
      send_emails: function() {
        if(this.group_name !== null && this.message_file_path !== null) {
          const url = this.localhost;
          const request_data = {
            action: 'sendEmails',
            groupname: this.group_name,
            subject: this.input_email_subject,
            messagepath: this.message_file_path,
            imagepath: this.image_file_path
          };
          const request_data_str=JSON.stringify(request_data);
          const config = {
            method: 'POST',
            mode: 'cors',
            body: request_data_str,
            headers: new Headers({
              'Content-Type': 'application/json',
              'Content-Length': request_data_str.length
            })
          };
          fetch(url, config).then(response => {
            if(response.ok) {
              return response.text();
            }
            throw new Error(response.statusText);
          }).then(resp_str => {
            this.email_bus.$emit('status_changed', resp_str);
          }).catch(error => {
            this.email_bus.$emit('status_changed', `Send emails error: ${error.message}`)
          });
        }
      }
    }
  }
</script>

<style lang="less">
  .emailComp {
    display: flex;
    flex-direction: column;
    background-color: darkslateblue;
    padding: 10px;
    height: 100%;
    font-family: Verdana, serif;

    &_emailsSec {
      display: flex;
      flex-direction: row;
      margin-top: 20px;
    }

    &_tableSec {
      display: flex;
      flex-direction: column;
      align-items: center;
    }

    &_databaseSec {
      display: flex;
      flex-direction: row;
      justify-content: space-between;
      width: 800px;
    }

    &_tableComp {
      margin-top: 30px;
    }

    &_inputSec {
      display: flex;
      flex-direction: row;
      justify-content:space-between;
      width: 700px;
      margin-top: 30px;
      align-items: center;
    }

    &_inputButtonSec {
      display: flex;
      flex-direction: row;
      justify-content:space-between;
      width: 300px;
      align-self:center;
      margin-top: 25px;
    }


    &_messageSec {
      display: flex;
      flex-direction: column;
      margin-left: 60px;
    }

    &_messageButtonSec {
      display: flex;
      flex-direction: row;
      justify-content: space-between;
      width: 600px;
    }

    &_infoSec {
      display: flex;
      flex-direction: column;
      justify-content: space-around;
      height: 100px;
      width: 660px;
      margin-top: 20px;
      padding: 10px;
      border: solid 1px white;
      border-radius: 5px;
    }


    &_statusSec {
      margin-top: 40px;
      font-size: 18px;
      width: 840px;
      color: white;
    }
  }
</style>